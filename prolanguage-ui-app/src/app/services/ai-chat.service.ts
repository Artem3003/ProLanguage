import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AiChatConversationMessages,
  AiChatConversationSummary,
  AiChatRequest,
  AiChatResponse,
} from '../models/ai-chat.model';

@Injectable({
  providedIn: 'root'
})
export class AiChatService {
  private readonly baseUrl = 'http://localhost:5201';

  constructor(private readonly http: HttpClient) { }

  getConversations(): Observable<AiChatConversationSummary[]> {
    return this.http.get<AiChatConversationSummary[]>(`${this.baseUrl}/aichat/conversations`);
  }

  getConversationMessages(conversationId: string): Observable<AiChatConversationMessages> {
    return this.http.get<AiChatConversationMessages>(`${this.baseUrl}/aichat/conversations/${conversationId}/messages`);
  }

  sendMessage(conversationId: string, message: string): Observable<AiChatResponse> {
    const payload: AiChatRequest = { message, conversationId };
    return this.http.post<AiChatResponse>(`${this.baseUrl}/aichat/messages`, payload);
  }

  deleteConversation(conversationId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/aichat/conversations/${conversationId}`);
  }
}
