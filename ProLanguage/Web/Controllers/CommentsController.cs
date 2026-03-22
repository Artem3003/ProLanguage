using System.Security.Claims;
using Application.DTOs.Comment;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Tags("Comments & Moderation")]
[Authorize]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    private readonly ICommentService _commentService = commentService;

    [HttpPost("courses/{id}/comments")]
    public async Task<ActionResult<Guid>> AddComment(Guid id, [FromBody] CreateCommentRequestDto request)
    {
        var currentUserName = ResolveCurrentUserDisplayName();
        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            return Unauthorized();
        }

        var commentId = await _commentService.AddCommentAsync(id, currentUserName, request);
        return CreatedAtAction(nameof(GetCommentsByCourseId), new { id }, commentId);
    }

    [HttpGet("courses/{id}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByCourseId(Guid id)
    {
        var currentUserName = ResolveCurrentUserDisplayName();
        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            return Unauthorized();
        }

        var comments = await _commentService.GetCommentsByCourseIdAsync(id, currentUserName);
        return Ok(comments);
    }

    [HttpDelete("courses/{courseId}/comments/{id}")]
    public async Task<ActionResult> DeleteComment(Guid courseId, Guid id)
    {
        var currentUserName = ResolveCurrentUserDisplayName();
        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            return Unauthorized();
        }

        await _commentService.DeleteCommentAsync(courseId, id, currentUserName);
        return NoContent();
    }

    [HttpGet("comments/ban/durations")]
    public ActionResult<List<string>> GetBanDurations()
    {
        var durations = _commentService.GetBanDurations();
        return Ok(durations);
    }

    [HttpPost("comments/ban")]
    [Authorize(Policy = "ContentManagement")]
    public async Task<ActionResult> BanUser([FromBody] BanRequestDto request)
    {
        await _commentService.BanUserAsync(request);
        return Ok();
    }

    private string? ResolveCurrentUserDisplayName()
    {
        var firstName =
            User.FindFirstValue("firstName")?.Trim() ??
            User.FindFirstValue(ClaimTypes.GivenName)?.Trim() ??
            User.FindFirstValue("given_name")?.Trim();

        var lastName =
            User.FindFirstValue("lastName")?.Trim() ??
            User.FindFirstValue(ClaimTypes.Surname)?.Trim() ??
            User.FindFirstValue("family_name")?.Trim();
        var fullName = User.FindFirstValue("fullName")?.Trim();
        var nameClaim = User.FindFirstValue(ClaimTypes.Name)?.Trim();

        return !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName)
            ? $"{firstName} {lastName}"
            : !string.IsNullOrWhiteSpace(fullName)
                ? fullName
                : !string.IsNullOrWhiteSpace(nameClaim) && !nameClaim.Contains('@')
                    ? nameClaim
                    : null;
    }
}
