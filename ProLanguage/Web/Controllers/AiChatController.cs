using System.Security.Claims;
using Application.DTOs.AiChat;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("AI Chat")]
[Authorize]
public class AiChatController(IAiChatService aiChatService, ILogger<AiChatController> logger) : ControllerBase
{
    private readonly IAiChatService _aiChatService = aiChatService;
    private readonly ILogger<AiChatController> _logger = logger;

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(IEnumerable<AiChatConversationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AiChatConversationSummaryDto>>> GetConversations(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var conversations = await _aiChatService.GetConversationsAsync(userId.Value, cancellationToken);
        return Ok(conversations);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(AiChatConversationMessagesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AiChatConversationMessagesDto>> GetConversationMessages(Guid conversationId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var conversation = await _aiChatService.GetConversationMessagesAsync(userId.Value, conversationId, cancellationToken);
            return Ok(conversation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("messages")]
    [ProducesResponseType(typeof(AiChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AiChatResponseDto>> SendMessage([FromBody] AiChatRequestDto request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        Application.Metrics.BusinessMetrics.ActiveChatSessions.Inc();
        try
        {
            var response = await _aiChatService.SendMessageAsync(userId.Value, request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "AI chat configuration error");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "AI chat is not configured correctly." });
        }
        finally
        {
            Application.Metrics.BusinessMetrics.ActiveChatSessions.Dec();
        }
    }

    [HttpDelete("conversations/{conversationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversation(Guid conversationId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _aiChatService.DeleteConversationAsync(userId.Value, conversationId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId) ? null : userId;
    }
}
