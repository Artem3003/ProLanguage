using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Data;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Services;

/// <summary>
/// Service for user management operations.
/// </summary>
/// <param name="userManager">The user manager.</param>
/// <param name="context">The database context.</param>
/// <param name="logger">The logger.</param>
public class UserService(
    UserManager<ApplicationUser> userManager,
    AuthDbContext context,
    ILogger<UserService> logger) : IUserService
{
    /// <inheritdoc />
    public async Task<UserProfileDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return MapToUserProfileDto(user, roles);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetUserByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return MapToUserProfileDto(user, roles);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10)
    {
        var users = await context.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(MapToUserProfileDto(user, roles));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to update profile for user {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        logger.LogInformation("Profile updated for user {UserId}", userId);

        var roles = await userManager.GetRolesAsync(user);
        return MapToUserProfileDto(user, roles);
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
        {
            logger.LogWarning("Password change failed for user {UserId}: passwords do not match", userId);
            return false;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        logger.LogInformation("Password changed for user {UserId}", userId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return false;
        }

        logger.LogInformation("User {UserId} deactivated", userId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return false;
        }

        logger.LogInformation("User {UserId} activated", userId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to delete user {UserId}: {Errors}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        logger.LogInformation("User {UserId} deleted", userId);

        return true;
    }

    private static UserProfileDto MapToUserProfileDto(ApplicationUser user, IEnumerable<string> roles)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
        };
    }
}
