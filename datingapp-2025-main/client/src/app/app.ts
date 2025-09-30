import { Component, inject, OnInit, effect } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { Router, RouterOutlet } from '@angular/router';
import { ConfirmDialog } from "../shared/confirm-dialog/confirm-dialog";
import { ChatbotComponent } from '../features/chatbot/chat/chat';
import { PresenceService } from '../core/services/presence-service';
import { AccountService } from '../core/services/account-service';

@Component({
  selector: 'app-root',
  imports: [Nav, RouterOutlet, ConfirmDialog, ChatbotComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected router = inject(Router);
  protected chatbotVisible = false;
  private presenceService = inject(PresenceService);
  private accountService = inject(AccountService);

  constructor() {
    // S'assurer que le PresenceService est initialisé quand l'utilisateur se connecte
    effect(() => {
      const user = this.accountService.currentUser();
      if (user && !this.presenceService.hubConnection) {
        console.log('🔄 Initialisation globale du PresenceService depuis App');
        this.presenceService.createHubConnection(user);
      }
    });
  }

  ngOnInit() {
    console.log('🚀 Application démarrée - Notifications globales activées');
  }

  toggleChatbot() {
    this.chatbotVisible = !this.chatbotVisible;
  }
}
