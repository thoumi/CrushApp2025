import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PaginatedResult } from '../../types/pagination';
import { Message } from '../../types/message';
import { AccountService } from './account-service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastService } from './toast-service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private baseUrl = environment.apiUrl;
  private hubUrl = environment.hubUrl;
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  private toast = inject(ToastService);
  private hubConnection?: HubConnection;
  messageThread = signal<Message[]>([]);
  typingUsers = signal<Set<string>>(new Set());
  typingUsersInfo = signal<Map<string, {id: string, displayName: string, imageUrl: string}>>(new Map());

  createHubConnection(otherUserId: string) {
    const currentUser = this.accountService.currentUser();
    if (!currentUser) return;
    
    // Vérifier si une connexion existe déjà
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      console.log('⚠️ MessageService déjà connecté, arrêt de la connexion existante');
      this.stopHubConnection();
    }
    
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'messages?userId=' + otherUserId, {
        accessTokenFactory: () => currentUser.token
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          if (retryContext.previousRetryCount === 0) {
            return 0;
          }
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        }
      })
      .build();

    // Gestion des événements de connexion
    this.hubConnection.onclose(error => {
      if (error) {
        console.error('❌ MessageService déconnecté avec erreur:', error);
      } else {
        console.log('🔌 MessageService déconnecté normalement');
      }
    });

    this.hubConnection.onreconnecting(error => {
      console.log('🔄 MessageService en cours de reconnexion...', error);
    });

    this.hubConnection.onreconnected(connectionId => {
      console.log('✅ MessageService reconnecté avec succès');
    });

    this.hubConnection.start()
      .then(() => {
        console.log('✅ MessageService connecté');
      })
      .catch(error => {
        console.error('❌ Erreur de connexion MessageService:', error);
        // Retry après 5 secondes
        setTimeout(() => {
          if (this.hubConnection?.state !== HubConnectionState.Connected) {
            console.log('🔄 Tentative de reconnexion MessageService...');
            this.hubConnection?.start().catch(err => console.error('❌ Échec de la reconnexion:', err));
          }
        }, 5000);
      });

    this.hubConnection.on('ReceiveMessageThread', (messages: Message[]) => {
      this.messageThread.set(messages.map(message => ({
          ...message,
          currentUserSender: message.senderId !== otherUserId
        })))
    });

    this.hubConnection.on('NewMessage', (message: Message) => {
      message.currentUserSender = message.senderId === currentUser.id;
      this.messageThread.update(messages => [...messages, message])
    });

    this.hubConnection.on('UserTyping', (userId: string, isTyping: boolean, userInfo?: {displayName: string, imageUrl: string}) => {
      console.log('UserTyping event received:', { userId, isTyping, userInfo });
      
      this.typingUsers.update(users => {
        const newUsers = new Set(users);
        if (isTyping) {
          newUsers.add(userId);
        } else {
          newUsers.delete(userId);
        }
        return newUsers;
      });

      this.typingUsersInfo.update(info => {
        const newInfo = new Map(info);
        if (isTyping && userInfo) {
          newInfo.set(userId, {
            id: userId,
            displayName: userInfo.displayName,
            imageUrl: userInfo.imageUrl
          });
          console.log('Added typing user info:', { userId, userInfo });
        } else {
          newInfo.delete(userId);
          console.log('Removed typing user:', userId);
        }
        return newInfo;
      });
    });
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error))
    }
    this.hubConnection = undefined;
  }

  getMessages(container: string, pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);
    params = params.append('container', container);

    return this.http.get<PaginatedResult<Message>>(this.baseUrl + 'messages', {params});
  }

  getMessageThread(memberId: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + memberId);
  }

  sendMessage(recipientId: string, content: string) {
    if (this.hubConnection?.state !== HubConnectionState.Connected) {
      console.error('❌ Impossible d\'envoyer le message: connexion non établie');
      this.toast.error('Erreur de connexion. Veuillez réessayer.');
      return Promise.reject('Connection not established');
    }
    return this.hubConnection.invoke('SendMessage', {recipientId, content})
  }

  deleteMessage(id: string) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }

  sendTypingIndicator(recipientId: string, isTyping: boolean) {
    if (this.hubConnection?.state !== HubConnectionState.Connected) {
      console.error('❌ Impossible d\'envoyer l\'indicateur de frappe: connexion non établie');
      return Promise.reject('Connection not established');
    }
    return this.hubConnection.invoke('SendTypingIndicator', recipientId, isTyping);
  }
}
