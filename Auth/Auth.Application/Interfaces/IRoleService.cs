using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

/// <summary>
/// Interface for role management service.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Gets all available roles.
    /// </summary>
    /// <returns>A collection of role names.</returns>
    Task<IEnumerable<string>> GetAllRolesAsync();

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="dto">The assign role DTO.</param>
    /// <returns>True if the role was assigned successfully.</returns>
    Task<bool> AssignRoleAsync(AssignRoleDto dto);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleName">The role name.</param>
    /// <returns>True if the role was removed successfully.</returns>
    Task<bool> RemoveRoleAsync(Guid userId, string roleName);

    /// <summary>
    /// Gets the roles for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of role names.</returns>
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

    /// <summary>
    /// Gets users in a specific role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>A collection of user profiles.</returns>
    Task<IEnumerable<UserProfileDto>> GetUsersInRoleAsync(string roleName);

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <returns>True if the role was created successfully.</returns>
    Task<bool> CreateRoleAsync(string roleName, string? description = null);

    /// <summary>
    /// Deletes a role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>True if the role was deleted successfully.</returns>
    Task<bool> DeleteRoleAsync(string roleName);
}
