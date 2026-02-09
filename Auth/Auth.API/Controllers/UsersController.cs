using System.Security.Claims;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

/// <summary>
/// Controller for user management operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UsersController"/> class.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <returns>The user profile.</returns>
    /// <response code="200">Returns the user profile.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(userId.Value);
        return user == null ? (ActionResult<UserProfileDto>)NotFound() : (ActionResult<UserProfileDto>)Ok(user);
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user profile.</returns>
    /// <response code="200">Returns the user profile.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return user == null ? (ActionResult<UserProfileDto>)NotFound() : (ActionResult<UserProfileDto>)Ok(user);
    }

    /// <summary>
    /// Gets all users with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (default: 1).</param>
    /// <param name="pageSize">The page size (default: 10).</param>
    /// <returns>A list of user profiles.</returns>
    /// <response code="200">Returns the list of users.</response>
    /// <response code="403">Access denied.</response>
    [HttpGet]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
        return Ok(users);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="dto">The update profile DTO.</param>
    /// <returns>The updated user profile.</returns>
    /// <response code="200">Profile updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userService.UpdateProfileAsync(userId.Value, dto);
        return user == null ? (ActionResult<UserProfileDto>)BadRequest(new { message = "Failed to update profile." }) : (ActionResult<UserProfileDto>)Ok(user);
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="dto">The change password DTO.</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Invalid request or current password incorrect.</response>
    /// <response code="401">User not authenticated.</response>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _userService.ChangePasswordAsync(userId.Value, dto);
        return !result
            ? BadRequest(new { message = "Failed to change password. Please verify your current password." })
            : Ok(new { message = "Password changed successfully." });
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="200">User deactivated successfully.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var result = await _userService.DeactivateUserAsync(id);
        return !result ? NotFound() : Ok(new { message = "User deactivated successfully." });
    }

    /// <summary>
    /// Activates a user account.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="200">User activated successfully.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var result = await _userService.ActivateUserAsync(id);
        return !result ? NotFound() : Ok(new { message = "User activated successfully." });
    }

    /// <summary>
    /// Deletes a user account.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return !result ? NotFound() : NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId) ? null : userId;
    }
}
