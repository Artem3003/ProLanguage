using Application.DTOs.Comment;

namespace Application.Interfaces;

public interface ICommentService
{
    Task<Guid> AddCommentAsync(Guid courseId, CreateCommentRequestDto request);

    Task<IEnumerable<CommentDto>> GetCommentsByCourseIdAsync(Guid courseId);

    Task DeleteCommentAsync(Guid courseId, Guid commentId);

    Task BanUserAsync(BanRequestDto request);

    List<string> GetBanDurations();
}
