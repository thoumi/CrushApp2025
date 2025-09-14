export type ChatbotMessage = {
  sender: 'user' | 'bot';
  prompt: string;
  timestamp: Date;
}