using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;

/// <summary>
/// Represents a role in the English School Platform.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRole"/> class.
    /// </summary>
    public ApplicationRole()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationRole"/> class with a specified name.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    public ApplicationRole(string roleName)
        : base(roleName)
    {
    }

    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the date when the role was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
