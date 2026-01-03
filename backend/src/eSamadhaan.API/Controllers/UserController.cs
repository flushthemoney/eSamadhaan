using eSamadhaan.Application.DTOs.Auth;
using eSamadhaan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "SystemAdmin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new user (officer or admin)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequestDto request)
    {
        _logger.LogInformation("Creating new user with email: {Email}", request.Email);
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        _logger.LogInformation("Retrieving user with ID: {UserId}", id);
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    /// <summary>
    /// Get all users with optional filtering by role or department
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] string? role = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] bool? activeOnly = null)
    {
        _logger.LogInformation("Retrieving users with filters - Role: {Role}, DepartmentId: {DepartmentId}, ActiveOnly: {ActiveOnly}",
            role, departmentId, activeOnly);

        var users = await _userService.GetUsersWithFiltersAsync(role, departmentId, activeOnly);

        return Ok(users);
    }

    /// <summary>
    /// Update user details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequestDto request)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", id);
        await _userService.UpdateUserAsync(id, request.Name, request.Email, request.Role, request.DepartmentId);
        return NoContent();
    }

    /// <summary>
    /// Update user status (activate/deactivate)
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequestDto request)
    {
        _logger.LogInformation("Updating status for user with ID: {UserId} to {IsActive}", id, request.IsActive);
        await _userService.UpdateUserStatusAsync(id, request.IsActive);
        return NoContent();
    }

    /// <summary>
    /// Get user count by role (reporting)
    /// </summary>
    [HttpGet("stats/by-role")]
    public async Task<ActionResult<Dictionary<string, int>>> GetUserCountByRole()
    {
        _logger.LogInformation("Retrieving user count by role");
        var stats = await _userService.GetUserCountByRoleAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Get officer count by department (reporting)
    /// </summary>
    [HttpGet("stats/by-department")]
    public async Task<ActionResult<Dictionary<int, int>>> GetOfficerCountByDepartment()
    {
        _logger.LogInformation("Retrieving officer count by department");
        var stats = await _userService.GetOfficerCountByDepartmentAsync();
        return Ok(stats);
    }
}
