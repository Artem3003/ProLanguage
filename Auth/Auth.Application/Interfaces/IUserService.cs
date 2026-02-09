using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

/// <summary>
/// Interface for user management service.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user profile by ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user profile.</returns>
    Task<UserProfileDto?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Gets a user profile by email.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>The user profile.</returns>
    Task<UserProfileDto?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A collection of user profiles.</returns>
    Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Updates a user profile.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="dto">The update profile DTO.</param>
    /// <returns>The updated user profile.</returns>
    Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="dto">The change password DTO.</param>
    /// <returns>True if the password was changed successfully.</returns>
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>True if the account was deactivated successfully.</returns>
    Task<bool> DeactivateUserAsync(Guid userId);

    /// <summary>
    /// Activates a user account.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>True if the account was activated successfully.</returns>
    Task<bool> ActivateUserAsync(Guid userId);

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>True if the account was deleted successfully.</returns>
    Task<bool> DeleteUserAsync(Guid userId);
}
