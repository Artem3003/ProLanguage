namespace Application.DTOs.AiChat;

public class AiChatHistoryResponseDto
{
    public List<AiChatConversationSummaryDto> Conversations { get; set; } = [];
}