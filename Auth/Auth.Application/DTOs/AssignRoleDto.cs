namespace Auth.Application.DTOs;

/// <summary>
/// DTO for assigning a role to a user.
/// </summary>
public class AssignRoleDto
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the role name.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}
