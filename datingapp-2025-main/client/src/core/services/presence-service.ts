import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { ToastService } from './toast-service';
import { User } from '../../types/user';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { Message } from '../../types/message';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  private hubUrl = environment.hubUrl;
  private toast = inject(ToastService);
  hubConnection?: HubConnection;
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: User) {
    console.log('🔗 Initialisation de la connexion PresenceService pour les notifications globales');
    
    // Demander la permission pour les notifications du navigateur
    this.requestNotificationPermission();

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('✅ PresenceService connecté - Notifications globales activées');
      })
      .catch(error => {
        console.error('❌ Erreur de connexion PresenceService:', error);
      });

    this.hubConnection.on('UserOnline', userId => {
      this.onlineUsers.update(users => [...users, userId])
    })

    this.hubConnection.on('UserOffline', userId => {
      this.onlineUsers.update(users => users.filter(x => x !== userId))
    });

    this.hubConnection.on('GetOnlineUsers', userIds => {
      this.onlineUsers.set(userIds);
    });

    this.hubConnection.on('NewMessageReceived', (message: Message) => {
      console.log('📨 Notification reçue globalement:', message);
      // Notification plus visible avec son et vibration
      this.showMessageNotification(message);
    })
  }

  private showMessageNotification(message: Message) {
    console.log('🔔 Affichage de la notification pour:', message.senderDisplayName);
    
    // Vibration si supportée
    if ('vibrate' in navigator) {
      navigator.vibrate([200, 100, 200]);
      console.log('📳 Vibration activée');
    }

    // Son de notification (optionnel)
    this.playNotificationSound();

    // Notification toast améliorée
    const notificationMessage = `💬 ${message.senderDisplayName} vous a envoyé un message`;
    this.toast.info(notificationMessage, 8000, message.senderImageUrl, `/members/${message.senderId}/messages`);
    console.log('🍞 Notification toast affichée');

    // Notification du navigateur si autorisée
    this.showBrowserNotification(message);
  }

  private playNotificationSound() {
    try {
      // Créer un son de notification simple
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      const oscillator = audioContext.createOscillator();
      const gainNode = audioContext.createGain();
      
      oscillator.connect(gainNode);
      gainNode.connect(audioContext.destination);
      
      oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
      oscillator.frequency.setValueAtTime(600, audioContext.currentTime + 0.1);
      
      gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
      gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);
      
      oscillator.start(audioContext.currentTime);
      oscillator.stop(audioContext.currentTime + 0.2);
    } catch (error) {
      console.log('Audio not supported');
    }
  }

  private requestNotificationPermission() {
    if ('Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission().then(permission => {
        if (permission === 'granted') {
          console.log('Notifications autorisées');
        }
      });
    }
  }

  private showBrowserNotification(message: Message) {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification(`Nouveau message de ${message.senderDisplayName}`, {
        body: message.content,
        icon: message.senderImageUrl || '/user.png',
        tag: `message-${message.senderId}`,
        requireInteraction: true
      });
    }
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error))
    }
  }
}
