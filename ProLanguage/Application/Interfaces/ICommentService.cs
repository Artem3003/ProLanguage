using Application.DTOs.Comment;

namespace Application.Interfaces;

public interface ICommentService
{
    Task<Guid> AddCommentAsync(Guid courseId, string currentUserName, CreateCommentRequestDto request);

    Task<IEnumerable<CommentDto>> GetCommentsByCourseIdAsync(Guid courseId, string currentUserName);

    Task DeleteCommentAsync(Guid courseId, Guid commentId, string currentUserName);

    Task BanUserAsync(BanRequestDto request);

    List<string> GetBanDurations();
}
