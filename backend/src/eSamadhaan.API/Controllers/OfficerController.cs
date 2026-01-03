using eSamadhaan.Application.DTOs.Officer;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/officer")]
[Authorize(Roles = "DepartmentOfficer")]
public class OfficerController : ControllerBase
{
    private readonly IOfficerService _officerService;

    public OfficerController(IOfficerService officerService)
    {
        _officerService = officerService;
    }

    /// <summary>
    /// Get all grievances for the officer's department
    /// </summary>
    [HttpGet("grievances")]
    public async Task<IActionResult> GetDepartmentGrievances(
        [FromQuery] int? categoryId = null,
        [FromQuery] GrievanceStatus? status = null,
        [FromQuery] string? sortBy = null)
    {
        var officerId = GetCurrentUserId();
        var grievances = await _officerService.GetDepartmentGrievancesAsync(officerId, categoryId, status, sortBy);
        return Ok(grievances);
    }

    /// <summary>
    /// Get detailed information about a specific grievance including status history
    /// </summary>
    [HttpGet("grievances/{id}")]
    public async Task<IActionResult> GetGrievanceDetail(int id)
    {
        var officerId = GetCurrentUserId();
        var grievance = await _officerService.GetGrievanceDetailAsync(id, officerId);
        return Ok(grievance);
    }

    /// <summary>
    /// Change the status of a grievance with optional internal remarks
    /// </summary>
    [HttpPut("grievances/{id}/status")]
    public async Task<IActionResult> ChangeGrievanceStatus(
        int id,
        [FromBody] ChangeStatusRequestDto request)
    {
        var officerId = GetCurrentUserId();
        await _officerService.ChangeGrievanceStatusAsync(id, officerId, request);
        return Ok(new { message = "Grievance status updated successfully" });
    }

    /// <summary>
    /// Get all officers in the current officer's department
    /// </summary>
    [HttpGet("/api/department/officers")]
    public async Task<IActionResult> GetDepartmentOfficers()
    {
        var officerId = GetCurrentUserId();
        var officers = await _officerService.GetDepartmentOfficersAsync(officerId);
        return Ok(officers);
    }

    // Helper method to extract user ID from claims
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }
}
