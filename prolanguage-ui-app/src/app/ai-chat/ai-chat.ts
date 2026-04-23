import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

interface ChatHistoryItem {
  id: string;
  title: string;
  updatedAt: string;
}

interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  sentAt: Date;
}

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-chat.html',
  styleUrl: './ai-chat.scss'
})
export class AiChatComponent {
  isSending = false;
  draftMessage = '';
  isSidebarOpen = true;

  readonly quickPrompts = [
    'Create a 7-day speaking plan',
    'Explain present perfect simply',
    'Give me IELTS writing tips'
  ];

  chatHistory: ChatHistoryItem[] = [
    { id: 'chat-1', title: 'English Grammar Help', updatedAt: 'Today' },
    { id: 'chat-2', title: 'Pricing and Study Plan', updatedAt: 'Yesterday' },
    { id: 'chat-3', title: 'Vocabulary Practice', updatedAt: '2 days ago' }
  ];

  activeChatId = 'chat-1';

  messagesByChat: Record<string, ChatMessage[]> = {
    'chat-1': [],
    'chat-2': [],
    'chat-3': []
  };

  constructor(private readonly router: Router) {}

  get activeMessages(): ChatMessage[] {
    return this.messagesByChat[this.activeChatId] ?? [];
  }

  get isMessagesWindowDisabled(): boolean {
    return this.activeMessages.some(message => message.role === 'user');
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  goHome(): void {
    this.router.navigateByUrl('/');
  }

  selectChat(chatId: string): void {
    this.activeChatId = chatId;
  }

  createNewChat(): void {
    const id = `chat-${crypto.randomUUID()}`;
    const newChat: ChatHistoryItem = {
      id,
      title: 'New chat',
      updatedAt: 'Now'
    };

    this.chatHistory = [newChat, ...this.chatHistory];
    this.messagesByChat[id] = [];
    this.activeChatId = id;
    this.draftMessage = '';
  }

  deleteChat(chatId: string, event: Event): void {
    event.stopPropagation();

    if (this.chatHistory.length === 1) {
      return;
    }

    this.chatHistory = this.chatHistory.filter(chat => chat.id !== chatId);
    delete this.messagesByChat[chatId];

    if (this.activeChatId === chatId) {
      this.activeChatId = this.chatHistory[0].id;
    }
  }

  sendQuickPrompt(prompt: string): void {
    this.draftMessage = prompt;
    this.sendMessage();
  }

  sendMessage(): void {
    const message = this.draftMessage.trim();
    if (!message || this.isSending) {
      return;
    }

    const userMessage: ChatMessage = {
      id: crypto.randomUUID(),
      role: 'user',
      content: message,
      sentAt: new Date()
    };

    this.messagesByChat[this.activeChatId] = [...this.activeMessages, userMessage];
    this.updateChatPreview(message);
    this.draftMessage = '';
    this.isSending = true;

    window.setTimeout(() => {
      const aiMessage: ChatMessage = {
        id: crypto.randomUUID(),
        role: 'assistant',
        content: this.generateAssistantReply(message),
        sentAt: new Date()
      };

      this.messagesByChat[this.activeChatId] = [...this.activeMessages, aiMessage];
      this.isSending = false;
    }, 550);
  }

  private updateChatPreview(latestMessage: string): void {
    this.chatHistory = this.chatHistory.map(chat => {
      if (chat.id !== this.activeChatId) {
        return chat;
      }

      return {
        ...chat,
        title: latestMessage.length > 28 ? `${latestMessage.slice(0, 28)}...` : latestMessage,
        updatedAt: 'Now'
      };
    });
  }

  private generateAssistantReply(userMessage: string): string {
    const normalized = userMessage.toLowerCase();

    if (normalized.includes('ielts')) {
      return 'Great goal. For IELTS improvement, focus on one Writing task structure daily, one timed Reading section, and 15 minutes of speaking self-recording. I can build a day-by-day plan if you share your target band.';
    }

    if (normalized.includes('grammar')) {
      return 'Let us simplify grammar: learn one rule, write 3 examples, then use the rule in 5 real-life sentences. If you want, I can quiz you right now.';
    }

    if (normalized.includes('plan')) {
      return 'Here is a simple study rhythm: 20 minutes vocabulary, 20 minutes grammar, 20 minutes speaking practice each day. Consistency beats intensity.';
    }

    return 'Nice question. I can help with explanations, examples, exercises, or a personalized study plan. Tell me your level and what you want to achieve this week.';
  }
}
