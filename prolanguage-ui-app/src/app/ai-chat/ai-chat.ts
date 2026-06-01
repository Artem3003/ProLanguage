import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AiChatService } from '../services/ai-chat.service';
import { AiChatConversationMessages, AiChatConversationSummary, AiChatMessageDto } from '../models/ai-chat.model';
import { AiContentPipe } from '../pipes/ai-content.pipe';

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
  imports: [CommonModule, FormsModule, AiContentPipe],
  templateUrl: './ai-chat.html',
  styleUrl: './ai-chat.scss'
})
export class AiChatComponent {
  isSending = false;
  draftMessage = '';
  isSidebarOpen = true;

  readonly quickPrompts = [
    'Tell me about ProLanguage platform',
    'What courses are there on ProLanguage?',
    'What should I do to start studying on ProLanguage?',
  ];
  // Start with a single empty chat. Users can create additional chats explicitly.
  chatHistory: ChatHistoryItem[] = [];

  activeChatId = '';

  messagesByChat: Record<string, ChatMessage[]> = {};

  // Track pending timers for delayed chat preview updates
  private pendingPreviewTimers: Record<string, number | null> = {};

  @ViewChild('messagesWindow', { static: false })
  private messagesWindowEl?: ElementRef<HTMLElement>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly aiChatService: AiChatService
  ) {}

  ngOnInit(): void {
    const query = this.route.snapshot.queryParamMap.get('q');
    if (query) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { q: null },
        queryParamsHandling: 'merge',
        replaceUrl: true
      });
    }
    this.loadConversations(query);
  }

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
    if (!this.messagesByChat[chatId]) {
      this.loadConversationMessages(chatId);
    }
  }

  createNewChat(): void {
    const id = crypto.randomUUID();
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

    // Call backend to delete the conversation
    this.aiChatService.deleteConversation(chatId).subscribe({
      next: () => {
        // Remove from local UI
        this.chatHistory = this.chatHistory.filter(chat => chat.id !== chatId);
        delete this.messagesByChat[chatId];

        if (this.activeChatId === chatId) {
          this.activeChatId = this.chatHistory[0].id;
        }
      },
      error: (err) => {
        console.error('Failed to delete conversation:', err);
        // Optionally show user feedback here
      }
    });
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

    const chatId = this.ensureValidConversationId(this.activeChatId);

    const userMessage: ChatMessage = {
      id: crypto.randomUUID(),
      role: 'user',
      content: message,
      sentAt: new Date()
    };
    this.messagesByChat[chatId] = [...this.activeMessages, userMessage];
    this.scrollToBottom();
    // Delay updating the chat preview so the UI updates after a short interaction pause
    this.scheduleUpdateChatPreview(chatId, message);
    this.draftMessage = '';
    this.isSending = true;

    // Add an assistant placeholder immediately for typing animation
    const placeholderId = crypto.randomUUID();
    const placeholderMessage: ChatMessage = {
      id: placeholderId,
      role: 'assistant',
      content: '',
      sentAt: new Date()
    };
    this.messagesByChat[chatId] = [...this.messagesByChat[chatId], placeholderMessage];
    this.scrollToBottom(false);

    this.aiChatService.sendMessage(chatId, message).subscribe({
      next: response => {
        // Progressively reveal the answer into the placeholder message
        this.clearPendingPreviewTimer(chatId);
        this.updateChatPreviewFromResponse(response.conversationId, response.conversationTitle, response.conversationUpdatedAtUtc);
        this.animateAssistantTyping(chatId, placeholderId, response.answer).finally(() => {
          this.isSending = false;
        });
      },
      error: () => {
        const fallback = 'I could not get a response from AI Tutor right now. Please try again in a moment.';
        const idx = (this.messagesByChat[chatId] ?? []).findIndex(m => m.id === placeholderId);
        if (idx >= 0) {
          (this.messagesByChat[chatId] ?? [])[idx].content = fallback;
        } else {
          this.messagesByChat[chatId] = [...(this.messagesByChat[chatId] ?? []), { id: crypto.randomUUID(), role: 'assistant', content: fallback, sentAt: new Date() }];
        }
        this.scrollToBottom();
        this.isSending = false;
      }
    });
  }

  private async animateAssistantTyping(conversationId: string, messageId: string, fullText: string): Promise<void> {
    const targetArr = Array.from(fullText);
    const speedMs = 18; // per character, tune for desired speed

    const messages = this.messagesByChat[conversationId];
    if (!messages) return;

    const idx = messages.findIndex(m => m.id === messageId);
    if (idx < 0) return;

    let current = '';
    for (let i = 0; i < targetArr.length; i++) {
      current += targetArr[i];
      messages[idx].content = current;
      this.scrollToBottom(false);
      // small await to allow UI to update
      // eslint-disable-next-line no-await-in-loop
      await new Promise(res => setTimeout(res, speedMs));
    }

    // final ensure full text set
    messages[idx].content = fullText;
    this.scrollToBottom();
  }

  private scheduleUpdateChatPreview(chatId: string, latestMessage: string, delay = 1500): void {
    // Clear any existing timer for this chat
    const existing = this.pendingPreviewTimers[chatId];
    if (existing) {
      clearTimeout(existing);
    }

    this.pendingPreviewTimers[chatId] = window.setTimeout(() => {
      this.updateChatPreview(latestMessage);
      this.pendingPreviewTimers[chatId] = null;
    }, delay) as unknown as number;
  }

  private clearPendingPreviewTimer(chatId: string): void {
    const t = this.pendingPreviewTimers[chatId];
    if (t) {
      clearTimeout(t);
      this.pendingPreviewTimers[chatId] = null;
    }
  }

  private updateChatPreview(latestMessage: string): void {
    this.updateChatPreviewForChat(this.activeChatId, latestMessage, 'Now');
  }

  private updateChatPreviewFromResponse(chatId: string, title: string, updatedAtUtc: string): void {
    this.updateChatPreviewForChat(chatId, title, this.formatUpdatedAt(updatedAtUtc));
  }

  private updateChatPreviewForChat(chatId: string, latestMessage: string, updatedAt: string): void {
    this.chatHistory = this.chatHistory.map(chat => {
      if (chat.id !== chatId) {
        return chat;
      }

      return {
        ...chat,
        title: latestMessage.length > 28 ? `${latestMessage.slice(0, 28)}...` : latestMessage,
        updatedAt,
      };
    });
  }

  private ensureValidConversationId(chatId: string): string {
    if (this.isGuid(chatId)) {
      return chatId;
    }

    const normalizedChatId = crypto.randomUUID();
    const chatIndex = this.chatHistory.findIndex(chat => chat.id === chatId);

    if (chatIndex >= 0) {
      const currentChat = this.chatHistory[chatIndex];
      this.chatHistory = this.chatHistory.map(chat => chat.id === chatId ? { ...chat, id: normalizedChatId } : chat);
      this.messagesByChat[normalizedChatId] = this.messagesByChat[chatId] ?? [];
      delete this.messagesByChat[chatId];

      if (this.activeChatId === chatId) {
        this.activeChatId = normalizedChatId;
      }

      this.updateChatPreviewForChat(normalizedChatId, currentChat.title, currentChat.updatedAt);
    }

    return normalizedChatId;
  }

  private isGuid(value: string): boolean {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
  }

  private loadConversations(initialQuery?: string | null): void {
    this.aiChatService.getConversations().subscribe({
      next: conversations => {
        this.chatHistory = conversations.map(conversation => this.mapConversationSummary(conversation));

        if (initialQuery) {
          this.createNewChat();
          setTimeout(() => this.sendQuickPrompt(initialQuery), 0);
          return;
        }

        if (this.chatHistory.length > 0) {
          this.activeChatId = this.chatHistory[0].id;
          this.loadConversationMessages(this.activeChatId);
          return;
        }

        this.createNewChat();
      },
      error: () => {
        this.createNewChat();
        if (initialQuery) {
          setTimeout(() => this.sendQuickPrompt(initialQuery), 0);
        }
      }
    });
  }

  private loadConversationMessages(chatId: string): void {
    this.aiChatService.getConversationMessages(chatId).subscribe({
      next: conversation => {
        this.chatHistory = this.chatHistory.map(chat => chat.id === conversation.conversationId
          ? {
              ...chat,
              title: conversation.title,
              updatedAt: this.formatUpdatedAt(conversation.updatedAtUtc)
            }
          : chat);

        this.messagesByChat[conversation.conversationId] = conversation.messages.map(message => this.mapMessageDto(message));
        this.scrollToBottom();
      }
    });
  }

  private mapConversationSummary(conversation: AiChatConversationSummary): ChatHistoryItem {
    return {
      id: conversation.id,
      title: conversation.title,
      updatedAt: this.formatUpdatedAt(conversation.updatedAtUtc)
    };
  }

  private mapMessageDto(message: AiChatMessageDto): ChatMessage {
    return {
      id: message.id,
      role: message.role,
      content: message.content,
      sentAt: new Date(message.sentAtUtc)
    };
  }

  private scrollToBottom(smooth = true): void {
    if (!this.messagesWindowEl) return;
    setTimeout(() => {
      try {
        const el = this.messagesWindowEl!.nativeElement;
        const top = el.scrollHeight;
        if (typeof (el as any).scrollTo === 'function') {
          try {
            (el as any).scrollTo({ top, behavior: smooth ? 'smooth' : 'auto' });
            return;
          } catch {
            // ignore and fallback
          }
        }
        el.scrollTop = top;
      } catch {
        // ignore
      }
    }, 0);
  }

  private formatUpdatedAt(updatedAtUtc: string): string {
    const updatedAt = new Date(updatedAtUtc);
    const now = new Date();
    const diffMs = now.getTime() - updatedAt.getTime();
    const diffHours = diffMs / (1000 * 60 * 60);

    if (diffHours < 24) {
      return 'Today';
    }

    if (diffHours < 48) {
      return 'Yesterday';
    }

    return updatedAt.toLocaleDateString([], { month: 'short', day: 'numeric' });
  }

}
