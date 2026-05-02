using Domain.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Migrations.Seeders;

public class BigDataSeeder(ILogger<BigDataSeeder> logger)
{
    private readonly ILogger<BigDataSeeder> _logger = logger;

    public async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        const int targetCourses = 100_000;
        const int targetLessons = 500_000;

        var existingCourses = await dbContext.Courses.AsNoTracking().CountAsync(cancellationToken);
        if (existingCourses >= targetCourses)
        {
            _logger.LogInformation("BigDataSeeder: required courses already present ({Count}). Skipping seeding.", existingCourses);
            return;
        }

        _logger.LogInformation("BigDataSeeder: starting seed - target courses: {Courses}, target lessons: {Lessons}", targetCourses, targetLessons);

        var rnd = new Random(12345);
        var coursesBatchSize = 2000; // create in batches to avoid high memory usage
        var lessonsBatch = new List<Lesson>();

        var coursesToCreate = targetCourses - existingCourses;
        var lessonsRemaining = targetLessons - await dbContext.Lessons.AsNoTracking().CountAsync(cancellationToken);

        for (int offset = 0; offset < coursesToCreate; offset += coursesBatchSize)
        {
            var batchCount = Math.Min(coursesBatchSize, coursesToCreate - offset);
            var courseList = new List<Course>(batchCount);

            for (int i = 0; i < batchCount; i++)
            {
                var courseId = Guid.NewGuid();

                var numberOfLessons = rnd.Next(1, 8); // average ~4

                // ensure we don't exceed lessonsRemaining
                if (lessonsRemaining <= 0)
                {
                    numberOfLessons = 0;
                }

                if (numberOfLessons > lessonsRemaining)
                {
                    numberOfLessons = lessonsRemaining;
                }

                var course = new Course
                {
                    Id = courseId,
                    Title = $"Course {offset + i + 1} - {Guid.NewGuid().ToString()[..8]}",
                    Description = "Auto-generated course for performance testing",
                    Price = Math.Round(rnd.NextDouble() * 200, 2),
                    NumberOfLessons = numberOfLessons,
                    Language = (Domain.Entities.Enums.CourseLanguage)rnd.Next(0, 3),
                    Level = (Domain.Entities.Enums.CourseLevel)rnd.Next(0, 3),
                    DurationMinutes = rnd.Next(30, 600),
                    Rating = Math.Round(rnd.NextDouble() * 5, 2),
                    IsOnSale = rnd.NextDouble() > 0.8,
                    DiscountPrice = null,
                    ViewCount = rnd.Next(0, 10000),
                    CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 365)),
                };

                courseList.Add(course);

                for (int l = 0; l < numberOfLessons; l++)
                {
                    var lesson = new Lesson
                    {
                        Id = Guid.NewGuid(),
                        CourseId = courseId,
                        Title = $"Lesson {l + 1} - {course.Title}",
                        Description = "Auto-generated lesson",
                        DurationMinutes = rnd.Next(5, 90),
                        Type = Domain.Entities.Enums.LessonType.Group,
                        Status = Domain.Entities.Enums.LessonStatus.Scheduled,
                        CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 365)),
                    };

                    lessonsBatch.Add(lesson);
                }

                lessonsRemaining -= numberOfLessons;
            }

            // Insert batch
            await dbContext.Courses.AddRangeAsync(courseList, cancellationToken);
            if (lessonsBatch.Count > 0)
            {
                await dbContext.Lessons.AddRangeAsync(lessonsBatch, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();

            _logger.LogInformation("BigDataSeeder: inserted {Courses} courses, {Lessons} lessons so far", Math.Min(offset + coursesBatchSize, coursesToCreate), targetLessons - lessonsRemaining);

            lessonsBatch.Clear();

            if (lessonsRemaining <= 0 && (offset + coursesBatchSize) >= coursesToCreate)
            {
                break;
            }
        }

        _logger.LogInformation("BigDataSeeder: seeding complete.");
    }
}
