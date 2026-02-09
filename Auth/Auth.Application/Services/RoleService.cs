using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Services;

/// <summary>
/// Service for role management operations.
/// </summary>
/// <param name="roleManager">The role manager.</param>
/// <param name="userManager">The user manager.</param>
/// <param name="logger">The logger.</param>
public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ILogger<RoleService> logger) : IRoleService
{
    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        return await roleManager.Roles
            .Select(r => r.Name!)
            .Where(n => n != null)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> AssignRoleAsync(AssignRoleDto dto)
    {
        var user = await userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
        {
            logger.LogWarning("Failed to assign role: user {UserId} not found", dto.UserId);
            return false;
        }

        var roleExists = await roleManager.RoleExistsAsync(dto.RoleName);
        if (!roleExists)
        {
            logger.LogWarning("Failed to assign role: role {RoleName} does not exist", dto.RoleName);
            return false;
        }

        var isInRole = await userManager.IsInRoleAsync(user, dto.RoleName);
        if (isInRole)
        {
            logger.LogInformation("User {UserId} is already in role {RoleName}", dto.UserId, dto.RoleName);
            return true;
        }

        var result = await userManager.AddToRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to assign role {RoleName} to user {UserId}: {Errors}", dto.RoleName, dto.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        logger.LogInformation("Role {RoleName} assigned to user {UserId}", dto.RoleName, dto.UserId);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveRoleAsync(Guid userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var isInRole = await userManager.IsInRoleAsync(user, roleName);
        if (!isInRole)
        {
            return true;
        }

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to remove role {RoleName} from user {UserId}: {Errors}", roleName, userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        logger.LogInformation("Role {RoleName} removed from user {UserId}", roleName, userId);

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user == null ? [] : await userManager.GetRolesAsync(user);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserProfileDto>> GetUsersInRoleAsync(string roleName)
    {
        var users = await userManager.GetUsersInRoleAsync(roleName);

        List<UserProfileDto> result = [];
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserProfileDto
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
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> CreateRoleAsync(string roleName, string? description = null)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (roleExists)
        {
            logger.LogWarning("Role {RoleName} already exists", roleName);
            return false;
        }

        var role = new ApplicationRole(roleName)
        {
            Description = description,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        logger.LogInformation("Role {RoleName} created", roleName);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteRoleAsync(string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return false;
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to delete role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        logger.LogInformation("Role {RoleName} deleted", roleName);

        return true;
    }
}
