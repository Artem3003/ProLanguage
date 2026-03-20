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
        var commentId = await _commentService.AddCommentAsync(id, request);
        return CreatedAtAction(nameof(GetCommentsByCourseId), new { id }, commentId);
    }

    [HttpGet("courses/{id}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByCourseId(Guid id)
    {
        var comments = await _commentService.GetCommentsByCourseIdAsync(id);
        return Ok(comments);
    }

    [HttpDelete("courses/{courseId}/comments/{id}")]
    public async Task<ActionResult> DeleteComment(Guid courseId, Guid id)
    {
        await _commentService.DeleteCommentAsync(courseId, id);
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
}
