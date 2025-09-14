import { Component, inject } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { Router, RouterOutlet } from '@angular/router';
import { ConfirmDialog } from "../shared/confirm-dialog/confirm-dialog";
import { ChatbotComponent } from '../features/chatbot/chat/chat';

@Component({
  selector: 'app-root',
  imports: [Nav, RouterOutlet, ConfirmDialog, ChatbotComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected router = inject(Router);
  protected chatbotVisible = false;
     toggleChatbot() {
    this.chatbotVisible = !this.chatbotVisible;
  }
}
