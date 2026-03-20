using Application.DTOs.Comment;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CommentService(
    IUnitOfWork unitOfWork,
    ICommentRepository commentRepository,
    IBanRepository banRepository,
    ILogger<CommentService> logger) : ICommentService
{
    private const string DeletedCommentMessage = "A comment/quote was deleted";

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICommentRepository _commentRepository = commentRepository;
    private readonly IBanRepository _banRepository = banRepository;
    private readonly ILogger<CommentService> _logger = logger;

    private static readonly List<string> BanDurations =
    [
        "1 hour",
        "1 day",
        "1 week",
        "1 month",
        "permanent"
    ];

    public async Task<Guid> AddCommentAsync(Guid courseId, CreateCommentRequestDto request)
    {
        _logger.LogInformation("Adding comment to course {CourseId} by {Name}", courseId, request.Comment.Name);

        // Check if user is banned
        var activeBan = await _banRepository.GetActiveBanByUserNameAsync(request.Comment.Name);
        if (activeBan != null)
        {
            _logger.LogWarning("User {Name} is banned until {ExpiresAt}", request.Comment.Name, activeBan.ExpiresAt?.ToString() ?? "permanent");
            throw new InvalidOperationException($"User '{request.Comment.Name}' is banned and cannot add comments.");
        }

        var comment = new Comment
        {
            Name = request.Comment.Name,
            Body = request.Comment.Body,
            CourseId = courseId,
            ParentCommentId = request.ParentId,
            CreatedAt = DateTime.UtcNow,
        };

        // Handle reply action - format body with author name
        if (request.Action == "reply" && request.ParentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentId.Value);
            if (parentComment == null)
            {
                throw new KeyNotFoundException($"Parent comment with ID {request.ParentId.Value} not found.");
            }

            // Body already formatted by client as [Author], text
            comment.Body = request.Comment.Body;
        }

        // Handle quote action - format body with quoted text
        if (request.Action == "quote" && request.ParentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentId.Value);
            if (parentComment == null)
            {
                throw new KeyNotFoundException($"Parent comment with ID {request.ParentId.Value} not found.");
            }

            // Body already formatted by client as [Body], text
            comment.Body = request.Comment.Body;
        }

        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully added comment with ID {CommentId} to course {CourseId}", comment.Id, courseId);

        return comment.Id;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByCourseIdAsync(Guid courseId)
    {
        _logger.LogInformation("Retrieving comments for course {CourseId}", courseId);

        var comments = await _commentRepository.GetByCourseIdAsync(courseId);
        var commentDtos = BuildCommentTree(comments);

        _logger.LogInformation("Successfully retrieved {Count} top-level comments for course {CourseId}", commentDtos.Count(), courseId);

        return commentDtos;
    }

    public async Task DeleteCommentAsync(Guid courseId, Guid commentId)
    {
        _logger.LogInformation("Deleting comment {CommentId} from course {CourseId}", commentId, courseId);

        var comment = await _commentRepository.GetByIdWithChildrenAsync(commentId);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} not found.");
        }

        if (comment.CourseId != courseId)
        {
            throw new InvalidOperationException("Comment does not belong to the specified course.");
        }

        // Soft delete - mark as deleted
        comment.IsDeleted = true;
        comment.Body = DeletedCommentMessage;
        _commentRepository.Update(comment);

        // Update any child comments that quote this deleted comment
        await UpdateQuotingComments(commentId);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted comment {CommentId}", commentId);
    }

    public async Task BanUserAsync(BanRequestDto request)
    {
        _logger.LogInformation("Banning user {User} for {Duration}", request.User, request.Duration);

        if (!BanDurations.Contains(request.Duration))
        {
            throw new InvalidOperationException($"Invalid ban duration: {request.Duration}. Valid durations: {string.Join(", ", BanDurations)}");
        }

        var ban = new Ban
        {
            UserName = request.User,
            Duration = request.Duration,
            BannedAt = DateTime.UtcNow,
            ExpiresAt = CalculateExpirationDate(request.Duration),
        };

        await _banRepository.AddAsync(ban);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully banned user {User} for {Duration}", request.User, request.Duration);
    }

    public List<string> GetBanDurations()
    {
        return BanDurations;
    }

    private IEnumerable<CommentDto> BuildCommentTree(IEnumerable<Comment> comments)
    {
        return comments.Select(MapCommentToDto);
    }

    private CommentDto MapCommentToDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Name = comment.Name,
            Body = comment.IsDeleted ? DeletedCommentMessage : comment.Body,
            ChildComments = comment.ChildComments
                .Select(MapCommentToDto)
                .ToList(),
        };
    }

    private async Task UpdateQuotingComments(Guid deletedCommentId)
    {
        var allComments = await _commentRepository.GetAllAsync();
        var quotingComments = allComments
            .Where(c => c.ParentCommentId == deletedCommentId && !c.IsDeleted);

        foreach (var quotingComment in quotingComments)
        {
            // If this comment was a quote, update the quoted text portion
            if (quotingComment.Body.StartsWith('['))
            {
                var closingBracket = quotingComment.Body.IndexOf(']');
                if (closingBracket > 0)
                {
                    quotingComment.Body = $"[{DeletedCommentMessage}]{quotingComment.Body[(closingBracket + 1)..]}";
                    _commentRepository.Update(quotingComment);
                }
            }
        }
    }

    private static DateTime? CalculateExpirationDate(string duration)
    {
        return duration switch
        {
            "1 hour" => DateTime.UtcNow.AddHours(1),
            "1 day" => DateTime.UtcNow.AddDays(1),
            "1 week" => DateTime.UtcNow.AddDays(7),
            "1 month" => DateTime.UtcNow.AddMonths(1),
            "permanent" => null,
            _ => throw new InvalidOperationException($"Invalid ban duration: {duration}"),
        };
    }
}
