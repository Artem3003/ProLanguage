using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

/// <summary>
/// Controller for role management operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RolesController"/> class.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    /// <summary>
    /// Gets all available roles.
    /// </summary>
    /// <returns>A list of role names.</returns>
    /// <response code="200">Returns the list of roles.</response>
    /// <response code="403">Access denied.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<string>>> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="dto">The assign role DTO.</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="200">Role assigned successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="403">Access denied.</response>
    [HttpPost("assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var result = await _roleService.AssignRoleAsync(dto);
        return !result
            ? BadRequest(new { message = "Failed to assign role. User or role may not exist." })
            : Ok(new { message = "Role assigned successfully." });
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleName">The role name.</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="200">Role removed successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="403">Access denied.</response>
    [HttpDelete("{userId:guid}/roles/{roleName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole(Guid userId, string roleName)
    {
        var result = await _roleService.RemoveRoleAsync(userId, roleName);
        return !result
            ? BadRequest(new { message = "Failed to remove role." })
            : Ok(new { message = "Role removed successfully." });
    }

    /// <summary>
    /// Gets the roles for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of role names.</returns>
    /// <response code="200">Returns the user's roles.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(Guid userId)
    {
        var roles = await _roleService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    /// <summary>
    /// Gets all users in a specific role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>A list of user profiles.</returns>
    /// <response code="200">Returns the users in the role.</response>
    /// <response code="403">Access denied.</response>
    [HttpGet("{roleName}/users")]
    [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetUsersInRole(string roleName)
    {
        var users = await _roleService.GetUsersInRoleAsync(roleName);
        return Ok(users);
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <param name="description">The role description (optional).</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="201">Role created successfully.</response>
    /// <response code="400">Role already exists.</response>
    /// <response code="403">Access denied.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRole([FromQuery] string roleName, [FromQuery] string? description = null)
    {
        var result = await _roleService.CreateRoleAsync(roleName, description);
        return !result
            ? BadRequest(new { message = "Failed to create role. Role may already exist." })
            : CreatedAtAction(nameof(GetAllRoles), new { message = "Role created successfully." });
    }

    /// <summary>
    /// Deletes a role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>Success or failure response.</returns>
    /// <response code="204">Role deleted successfully.</response>
    /// <response code="400">Failed to delete role.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">Role not found.</response>
    [HttpDelete("{roleName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(string roleName)
    {
        var result = await _roleService.DeleteRoleAsync(roleName);
        return !result ? NotFound(new { message = "Role not found." }) : NoContent();
    }
}
