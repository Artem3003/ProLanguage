export interface AiChatRequest {
  message: string;

  conversationId?: string;
}

export interface AiChatResponse {
  conversationId: string;
  answer: string;
  model: string;
  generatedAtUtc: string;
  conversationTitle: string;
  conversationUpdatedAtUtc: string;
}

export interface AiChatConversationSummary {
  id: string;
  title: string;
  updatedAtUtc: string;
}

export interface AiChatMessageDto {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  sentAtUtc: string;
}

export interface AiChatConversationMessages {
  conversationId: string;
  title: string;
  updatedAtUtc: string;
  messages: AiChatMessageDto[];
}
