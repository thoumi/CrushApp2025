import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http"; 
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ChatbotService {
    basUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    sendMessageBot(message: string){
     const requestBody = {
      model: "phi3",   
      prompt: message,
      stream: false
    };
    
    return this.http.post<{ response: string }>(
      this.basUrl + 'chatbot/ask',
      requestBody,
      { headers: { 'Content-Type': 'application/json' } }
    );
    }
}
