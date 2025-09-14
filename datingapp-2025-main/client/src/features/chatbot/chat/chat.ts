import { AfterViewChecked, Component, ElementRef, OnInit, ViewChild,ChangeDetectorRef, inject } from '@angular/core';
import { ChatbotMessage } from '../../../types/chatbot';
import { ChatbotService } from '../../../core/services/chatbot-service';
import { NgClass } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BusyService } from '../../../core/services/busy-service';



@Component({
    selector: 'app-chatbot',
    templateUrl: './chat.html',
    styleUrls: ['./chat.css'],
    imports: [NgClass, FormsModule]
})
export class ChatbotComponent implements OnInit, AfterViewChecked {

  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;
  protected busyService = inject(BusyService);


  messages: ChatbotMessage[] = [];
  currentMessage: string = '';
  isLoading: boolean = false;

  constructor(private chatbotService: ChatbotService, private cdr: ChangeDetectorRef) { }
  ngOnInit(): void {
    this.messages.push({
      sender: 'bot',
      prompt: 'Hi! I’m your Crush Helper coach. Ask me a question about how to improve your profile!',
      timestamp: new Date()
    });
  }

ngAfterViewChecked() {
  this.scrollToBottom();
  this.cdr.detectChanges();
}

  sendMessageBot() {
    if(!this.currentMessage.trim()) return;

    const userMessage:  ChatbotMessage = {
      sender: 'user',
      prompt: this.currentMessage,
      timestamp: new Date()
    }
    this.messages.push(userMessage);

    const messageToSend = this.currentMessage;
    this.currentMessage ='';
    this.isLoading= true;

    this.chatbotService.sendMessageBot(messageToSend).subscribe(response => {
    const botMessage: ChatbotMessage = {

         sender: 'bot',
        prompt: response.response,
        timestamp: new Date()
      };
      this.messages.push(botMessage);
      this.isLoading = false;
    }, error => {
            const errorMessage: ChatbotMessage = {
        sender: 'bot',
        prompt: 'Désolé, une erreur est survenue. Veuillez réessayer plus tard.',
        timestamp: new Date()
      };
      this.messages.push(errorMessage);
      this.isLoading = false;
     });
    }

    private scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch (err) { }
  }
}
