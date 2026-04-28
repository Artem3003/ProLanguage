using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Course> Courses { get; set; }

    public DbSet<Lesson> Lessons { get; set; }

    public DbSet<StudentLesson> StudentLessons { get; set; }

    public DbSet<Homework> Homeworks { get; set; }

    public DbSet<HomeworkAssignment> HomeworkAssignments { get; set; }

    public DbSet<CalendarEvent> CalendarEvents { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderCourse> OrderCourses { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<Ban> Bans { get; set; }

    public DbSet<AiChatConversation> AiChatConversations { get; set; }

    public DbSet<AiChatMessage> AiChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Course configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.NumberOfLessons).IsRequired();
            entity.Property(e => e.Language).IsRequired();
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Rating);
            entity.Property(e => e.IsOnSale).IsRequired();
            entity.Property(e => e.DiscountPrice);
            entity.Property(e => e.ViewCount).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Lesson configuration
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Lessons)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // StudentLesson configuration
        modelBuilder.Entity<StudentLesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttendanceStatus).IsRequired();
            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.StudentLessons)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Homework configuration
        modelBuilder.Entity<Homework>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Instructions).IsRequired();
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.Homeworks)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // HomeworkAssignment configuration
        modelBuilder.Entity<HomeworkAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired();
            entity.HasOne(e => e.Homework)
                  .WithMany(h => h.HomeworkAssignments)
                  .HasForeignKey(e => e.HomeworkId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CalendarEvent configuration
        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StartDateTime).IsRequired();
            entity.Property(e => e.EndDateTime).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.HasOne(e => e.Lesson)
                  .WithMany(l => l.CalendarEvents)
                  .HasForeignKey(e => e.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.Status).IsRequired()
                  .HasConversion<string>();
        });

        // OrderCourse configuration
        modelBuilder.Entity<OrderCourse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.CourseId).IsRequired();
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Discount);
            entity.HasIndex(e => new { e.OrderId, e.CourseId }).IsUnique();
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderCourses)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Course)
                  .WithMany()
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Comment configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.CourseId).IsRequired();
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.IsQuote).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.ParentComment)
                  .WithMany(e => e.ChildComments)
                  .HasForeignKey(e => e.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Course)
                  .WithMany()
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Ban configuration
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Duration).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BannedAt).IsRequired();
        });

        // AI chat conversation configuration
        modelBuilder.Entity<AiChatConversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.Property(e => e.UpdatedAtUtc).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.UpdatedAtUtc });
            entity.HasMany(e => e.Messages)
                  .WithOne(e => e.Conversation)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConversationId).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SentAtUtc).IsRequired();
            entity.HasIndex(e => new { e.ConversationId, e.SentAtUtc });
        });
    }
}
