using eSamadhaan.Application.DTOs.Auth;
using eSamadhaan.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var userId = await _authService.RegisterUserAsync(
            request.Name,
            request.Email,
            request.Password,
            request.Role,
            request.DepartmentId
        );

        var user = await _userService.GetUserByIdAsync(userId);

        return CreatedAtAction(
            nameof(GetProfile),
            new { id = userId },
            new
            {
                message = "User registered successfully",
                userId = userId,
                email = user.Email,
                role = user.Role
            }
        );
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);

        var user = await _userService.GetUserByEmailAsync(request.Email);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            Token = token
        };

        return Ok(response);
    }

    /// <summary>
    /// Get current user profile (authenticated users only)
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                          User.FindFirst("sub")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        var user = await _userService.GetUserByIdAsync(userId);

        return Ok(new
        {
            userId = user.Id,
            name = user.Name,
            email = user.Email,
            role = user.Role,
            departmentId = user.DepartmentId,
            isActive = user.IsActive
        });
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                          User.FindFirst("sub")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user token" });
        }

        await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Check if email is available for registration
    /// </summary>
    [HttpGet("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmailAvailability([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        var isAvailable = await _authService.IsEmailAvailableAsync(email);

        return Ok(new
        {
            email = email,
            isAvailable = isAvailable
        });
    }

}
