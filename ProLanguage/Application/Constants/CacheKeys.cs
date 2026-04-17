namespace Application.Constants;

public static class CacheKeys
{
    public const string Courses = "Courses";
    public const string CalendarEvents = "CalendarEvents";
    public const string HomeworkAssignments = "HomeworkAssignments";
    public const string Homework = "Homework";
    public const string Lessons = "Lessons";
    public const string TotalLessonsCount = "total-lessons-count";
    public const string TotalCoursesCount = "total-courses-count";
    public const string CourseLanguages = "course-languages";
    public const string CourseLevels = "course-levels";
    public const string CourseRatings = "course-ratings";
    public const string CourseImagePrefix = "course-image-";

    public static string CourseImage(Guid courseId) => $"{CourseImagePrefix}{courseId}";
}