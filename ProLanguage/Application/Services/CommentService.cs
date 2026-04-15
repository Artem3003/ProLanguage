using Application.DTOs.Comment;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CommentService(
    IUnitOfWork unitOfWork,
    ICommentRepository commentRepository,
    ICourseRepository courseRepository,
    IBanRepository banRepository,
    ILogger<CommentService> logger) : ICommentService
{
    private const string DeletedCommentMessage = "A comment/quote was deleted";

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICommentRepository _commentRepository = commentRepository;
    private readonly ICourseRepository _courseRepository = courseRepository;
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

    public async Task<Guid> AddCommentAsync(Guid courseId, string currentUserName, CreateCommentRequestDto request)
    {
        _logger.LogInformation("Adding comment to course {CourseId} by {Name}", courseId, currentUserName);

        if (string.IsNullOrWhiteSpace(currentUserName) || string.IsNullOrWhiteSpace(request.Comment.Body))
        {
            throw new InvalidOperationException("Comment body is required.");
        }

        if (request.Comment.Rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        // Check if user is banned
        var activeBan = await _banRepository.GetActiveBanByUserNameAsync(currentUserName);
        if (activeBan != null)
        {
            _logger.LogWarning("User {Name} is banned until {ExpiresAt}", currentUserName, activeBan.ExpiresAt?.ToString() ?? "permanent");
            throw new InvalidOperationException($"User '{currentUserName}' is banned and cannot add comments.");
        }

        var action = request.Action?.Trim().ToLowerInvariant();
        if (action is not null and not ("reply" or "quote"))
        {
            throw new InvalidOperationException("Action must be either 'reply' or 'quote'.");
        }

        if (action is not null && !request.ParentId.HasValue)
        {
            throw new InvalidOperationException("ParentId is required for reply and quote actions.");
        }

        Comment? parentComment = null;
        if (request.ParentId.HasValue)
        {
            parentComment = await _commentRepository.GetByIdAsync(request.ParentId.Value);
            if (parentComment == null)
            {
                throw new KeyNotFoundException($"Parent comment with ID {request.ParentId.Value} not found.");
            }

            if (parentComment.CourseId != courseId)
            {
                throw new InvalidOperationException("Parent comment does not belong to the specified course.");
            }
        }

        var commentBody = request.Comment.Body.Trim();
        var isQuote = false;

        if (action == "reply" && parentComment != null)
        {
            if (!string.Equals(parentComment.Name, currentUserName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("You can reply only to your own comments.");
            }

            var replyText = GetTrailingText(commentBody);
            commentBody = $"[{parentComment.Name}], {replyText}";
        }

        if (action == "quote" && parentComment != null)
        {
            if (!string.Equals(parentComment.Name, currentUserName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("You can quote only your own comments.");
            }

            var quoteText = GetTrailingText(commentBody);
            var quotedSource = parentComment.IsDeleted ? DeletedCommentMessage : parentComment.Body;
            commentBody = $"[{quotedSource}], {quoteText}";
            isQuote = true;
        }

        var comment = new Comment
        {
            Name = currentUserName.Trim(),
            Body = commentBody,
            CourseId = courseId,
            Rating = request.Comment.Rating,
            ParentCommentId = request.ParentId,
            IsQuote = isQuote,
            CreatedAt = DateTime.UtcNow,
        };

        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        await UpdateCourseRatingAsync(courseId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Successfully added comment with ID {CommentId} to course {CourseId}", comment.Id, courseId);

        return comment.Id;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByCourseIdAsync(Guid courseId, string currentUserName)
    {
        _logger.LogInformation("Retrieving comments for course {CourseId}", courseId);

        var comments = await _commentRepository.GetFlatByCourseIdAsync(courseId);
        var commentDtos = BuildCommentTree(comments, currentUserName);

        _logger.LogInformation("Successfully retrieved {Count} top-level comments for course {CourseId}", commentDtos.Count, courseId);

        return commentDtos;
    }

    public async Task DeleteCommentAsync(Guid courseId, Guid commentId, string currentUserName)
    {
        _logger.LogInformation("Deleting comment {CommentId} from course {CourseId}", commentId, courseId);

        var comment = await _commentRepository.GetByIdWithParentAsync(commentId);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} not found.");
        }

        if (comment.CourseId != courseId)
        {
            throw new InvalidOperationException("Comment does not belong to the specified course.");
        }

        if (!string.Equals(comment.Name, currentUserName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("You can delete only your own comments.");
        }

        // Soft delete - mark as deleted
        comment.IsDeleted = true;
        comment.Body = DeletedCommentMessage;
        _commentRepository.Update(comment);

        // Update any child comments that quote this deleted comment
        await UpdateQuotingComments(courseId, commentId);

        await _unitOfWork.SaveChangesAsync();

        await UpdateCourseRatingAsync(courseId);
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

    private static string GetTrailingText(string body)
    {
        var markerIndex = body.IndexOf(", ", StringComparison.Ordinal);
        return body.StartsWith('[') && markerIndex > 0
            ? body[(markerIndex + 2)..].Trim()
            : body;
    }

    private static List<CommentDto> BuildCommentTree(List<Comment> comments, string currentUserName)
    {
        var dtoById = comments.ToDictionary(c => c.Id, c => MapCommentToDto(c, currentUserName));
        var rootComments = new List<CommentDto>();

        foreach (var comment in comments)
        {
            if (comment.ParentCommentId.HasValue && dtoById.TryGetValue(comment.ParentCommentId.Value, out var parentDto))
            {
                parentDto.ChildComments.Add(dtoById[comment.Id]);
            }
            else
            {
                rootComments.Add(dtoById[comment.Id]);
            }
        }

        return rootComments;
    }

    private static CommentDto MapCommentToDto(Comment comment, string currentUserName)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Name = comment.Name,
            CreatedAt = comment.CreatedAt,
            Rating = comment.Rating,
            Body = comment.IsDeleted ? DeletedCommentMessage : comment.Body,
            IsOwnComment = string.Equals(comment.Name, currentUserName, StringComparison.OrdinalIgnoreCase),
            ChildComments = [],
        };
    }

    private async Task UpdateQuotingComments(Guid courseId, Guid deletedCommentId)
    {
        var allComments = await _commentRepository.GetFlatByCourseIdAsync(courseId);
        var quotingComments = allComments
            .Where(c => c.ParentCommentId == deletedCommentId && c.IsQuote && !c.IsDeleted);

        foreach (var quotingComment in quotingComments)
        {
            if (quotingComment.Body.StartsWith('['))
            {
                var closingBracket = quotingComment.Body.IndexOf(']');
                if (closingBracket > 0)
                {
                    var trailing = quotingComment.Body[(closingBracket + 1)..];
                    quotingComment.Body = $"[{DeletedCommentMessage}]{trailing}";
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

    private async Task UpdateCourseRatingAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return;
        }

        var comments = await _commentRepository.GetFlatByCourseIdAsync(courseId);
        var ratedComments = comments
            .Where(c => !c.IsDeleted)
            .Select(c => c.Rating)
            .ToList();

        course.Rating = ratedComments.Count == 0
            ? null
            : ratedComments.Average();

        _courseRepository.Update(course);
    }
}
