import { Component, effect, ElementRef, inject, model, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
import { MessageService } from '../../../core/services/message-service';
import { MemberService } from '../../../core/services/member-service';
import { Message } from '../../../types/message';
import { DatePipe, KeyValuePipe } from '@angular/common';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { FormsModule } from '@angular/forms';
import { PresenceService } from '../../../core/services/presence-service';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Component({
  selector: 'app-member-messages',
  imports: [DatePipe, KeyValuePipe, TimeAgoPipe, FormsModule, TranslatePipe],
  templateUrl: './member-messages.html',
  styleUrl: './member-messages.css'
})
export class MemberMessages implements OnInit, OnDestroy {
  @ViewChild('messageEndRef') messageEndRef!: ElementRef
  protected messageService = inject(MessageService);
  private memberService = inject(MemberService);
  protected presenceService = inject(PresenceService);
  private route = inject(ActivatedRoute);
  protected messageContent = model('');
  private typingSubject = new Subject<string>();
  private typingTimeout?: any;

  constructor() {
    effect(() => {
      const currentMessages = this.messageService.messageThread();
      if (currentMessages.length > 0) {
        this.scrollToBottom();
      }
    });

    // Gérer les événements de typing avec debounce
    this.typingSubject.pipe(
      debounceTime(1000),
      distinctUntilChanged()
    ).subscribe(() => {
      this.stopTyping();
    });
  }

  ngOnInit(): void {
    this.route.parent?.paramMap.subscribe({
      next: params => {
        const otherUserId = params.get('id');
        if (!otherUserId) throw new Error('Cannot connect to hub');
        this.messageService.createHubConnection(otherUserId);
      }
    })
  }

  sendMessage() {
    const recipientId = this.memberService.member()?.id;
    if (!recipientId || !this.messageContent()) return;
    this.messageService.sendMessage(recipientId, this.messageContent())?.then(() => {
      this.messageContent.set('');
      this.stopTyping();
    })
  }

  onTyping() {
    const recipientId = this.memberService.member()?.id;
    if (!recipientId) return;

    // Envoyer l'indicateur de typing
    this.messageService.sendTypingIndicator(recipientId, true);
    
    // Annuler le timeout précédent
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
    
    // Programmer l'arrêt du typing après 3 secondes
    this.typingTimeout = setTimeout(() => {
      this.stopTyping();
    }, 3000);
  }

  private stopTyping() {
    const recipientId = this.memberService.member()?.id;
    if (!recipientId) return;

    this.messageService.sendTypingIndicator(recipientId, false);
    
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
      this.typingTimeout = undefined;
    }
  }

  scrollToBottom() {
    setTimeout(() => {
      if (this.messageEndRef) {
        this.messageEndRef.nativeElement.scrollIntoView({ behavior: 'smooth' })
      }
    })
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
  }

  get typingUsers() {
    return this.messageService.typingUsersInfo();
  }
}
