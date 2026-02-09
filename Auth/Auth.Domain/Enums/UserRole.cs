namespace Auth.Domain.Enums;

/// <summary>
/// Defines the available user roles in the English School Platform.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Student role - can attend lessons, submit homework.
    /// </summary>
    Student = 0,

    /// <summary>
    /// Teacher role - can manage lessons, create homework, grade assignments.
    /// </summary>
    Teacher = 1,

    /// <summary>
    /// Administrator role - has full access to the system.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Manager role - can manage courses, teachers, and students.
    /// </summary>
    Manager = 3,
}
