using eSamadhaan.Application.DTOs.Assignment;
using eSamadhaan.Application.DTOs.Grievance;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eSamadhaan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GrievanceController : ControllerBase
{
    private readonly IGrievanceService _grievanceService;
    private readonly IAssignmentService _assignmentService;

    public GrievanceController(
        IGrievanceService grievanceService,
        IAssignmentService assignmentService)
    {
        _grievanceService = grievanceService;
        _assignmentService = assignmentService;
    }

    /// <summary>
    /// Lodge a new grievance (Citizen only)
    /// </summary>
    [HttpPost("lodge")]
    [Authorize(Roles = "Citizen")]
    public async Task<IActionResult> LodgeGrievance([FromBody] CreateGrievanceRequestDto request)
    {
        var citizenId = GetCurrentUserId();

        var grievanceId = await _grievanceService.CreateGrievanceAsync(
            citizenId,
            request.CategoryId,
            request.DepartmentId,
            request.Description,
            request.AttachmentUrl);

        return CreatedAtAction(
            nameof(GetGrievanceById),
            new { id = grievanceId },
            new { grievanceId });
    }

    /// <summary>
    /// Assign grievance to an officer (DepartmentOfficer, SupervisoryOfficer, SystemAdmin)
    /// </summary>
    [HttpPost("{grievanceId}/assign")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> AssignGrievance(
        int grievanceId,
        [FromBody] CreateAssignmentRequestDto request)
    {
        var assignedByUserId = GetCurrentUserId();

        var assignmentId = await _assignmentService.AssignGrievanceAsync(
            grievanceId,
            request.OfficerId,
            assignedByUserId);

        return Ok(new { assignmentId, message = "Grievance assigned successfully" });
    }

    /// <summary>
    /// Update grievance status (DepartmentOfficer, SupervisoryOfficer, SystemAdmin)
    /// </summary>
    [HttpPut("{grievanceId}/status")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> UpdateGrievanceStatus(
        int grievanceId,
        [FromBody] ChangeGrievanceStatusRequestDto request)
    {
        var userId = GetCurrentUserId();

        await _grievanceService.ChangeGrievanceStatusAsync(
            grievanceId,
            request.NewStatus,
            userId,
            request.Remarks);

        return Ok(new { message = "Grievance status updated successfully" });
    }

    /// <summary>
    /// View grievance details by ID (All authenticated users)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGrievanceById(int id)
    {
        var grievance = await _grievanceService.GetGrievanceByIdAsync(id);

        // Apply authorization: Citizens can only view their own grievances
        if (User.IsInRole("Citizen"))
        {
            var citizenId = GetCurrentUserId();
            var grievanceDto = grievance as dynamic;
            
            if (grievanceDto?.CitizenId != citizenId)
            {
                return Forbid();
            }
        }

        return Ok(grievance);
    }

    /// <summary>
    /// View grievance details by grievance number (All authenticated users)
    /// </summary>
    [HttpGet("number/{grievanceNumber}")]
    public async Task<IActionResult> GetGrievanceByNumber(string grievanceNumber)
    {
        var grievance = await _grievanceService.GetGrievanceByNumberAsync(grievanceNumber);

        // Apply authorization: Citizens can only view their own grievances
        if (User.IsInRole("Citizen"))
        {
            var citizenId = GetCurrentUserId();
            var grievanceDto = grievance as dynamic;
            
            if (grievanceDto?.CitizenId != citizenId)
            {
                return Forbid();
            }
        }

        return Ok(grievance);
    }

    /// <summary>
    /// Get list of grievances for the current citizen
    /// </summary>
    [HttpGet("my-grievances")]
    [Authorize(Roles = "Citizen")]
    public async Task<IActionResult> GetMyGrievances()
    {
        var citizenId = GetCurrentUserId();

        var grievances = await _grievanceService.GetGrievancesByCitizenAsync(citizenId);

        return Ok(grievances);
    }

    /// <summary>
    /// Get grievance queue for officers (assigned to them)
    /// </summary>
    [HttpGet("my-queue")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer")]
    public async Task<IActionResult> GetMyQueue()
    {
        var officerId = GetCurrentUserId();

        var assignments = await _assignmentService.GetActiveAssignmentsByOfficerIdAsync(officerId);

        return Ok(assignments);
    }

    /// <summary>
    /// Get all grievances for a department (Officers and Admins)
    /// </summary>
    [HttpGet("department/{departmentId}")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetGrievancesByDepartment(int departmentId)
    {
        var grievances = await _grievanceService.GetGrievancesByDepartmentAsync(departmentId);

        return Ok(grievances);
    }

    /// <summary>
    /// Get all grievances (SystemAdmin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> GetAllGrievances()
    {
        var grievances = await _grievanceService.GetAllGrievancesAsync();

        return Ok(grievances);
    }

    /// <summary>
    /// Search grievances with filters (Officers and Admins)
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> SearchGrievances(
        [FromQuery] string? grievanceNumber = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] GrievanceStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var grievances = await _grievanceService.SearchGrievancesAsync(
            grievanceNumber,
            departmentId,
            categoryId,
            status,
            fromDate,
            toDate);

        return Ok(grievances);
    }

    /// <summary>
    /// Get grievance status history
    /// </summary>
    [HttpGet("{grievanceId}/history")]
    public async Task<IActionResult> GetGrievanceStatusHistory(int grievanceId)
    {
        // Apply authorization: Citizens can only view history of their own grievances
        if (User.IsInRole("Citizen"))
        {
            var citizenId = GetCurrentUserId();
            var grievance = await _grievanceService.GetGrievanceByIdAsync(grievanceId);
            var grievanceDto = grievance as dynamic;
            
            if (grievanceDto?.CitizenId != citizenId)
            {
                return Forbid();
            }
        }

        var history = await _grievanceService.GetGrievanceStatusHistoryAsync(grievanceId);

        return Ok(history);
    }

    /// <summary>
    /// Close a grievance (Officers and Admins)
    /// </summary>
    [HttpPost("{grievanceId}/close")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> CloseGrievance(int grievanceId)
    {
        var userId = GetCurrentUserId();

        await _grievanceService.CloseGrievanceAsync(grievanceId, userId);

        return Ok(new { message = "Grievance closed successfully" });
    }

    /// <summary>
    /// Reopen a closed grievance (Officers and Admins)
    /// </summary>
    [HttpPost("{grievanceId}/reopen")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> ReopenGrievance(
        int grievanceId,
        [FromBody] ReopenGrievanceRequestDto request)
    {
        var userId = GetCurrentUserId();

        await _grievanceService.ReopenGrievanceAsync(grievanceId, userId, request.Reason);

        return Ok(new { message = "Grievance reopened successfully" });
    }

    /// <summary>
    /// Get grievance count by status (Admins and Officers)
    /// </summary>
    [HttpGet("statistics/by-status")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetGrievanceCountByStatus()
    {
        var statistics = await _grievanceService.GetGrievanceCountByStatusAsync();

        return Ok(statistics);
    }

    /// <summary>
    /// Get grievance count by department (Admins and Officers)
    /// </summary>
    [HttpGet("statistics/by-department")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetGrievanceCountByDepartment()
    {
        var statistics = await _grievanceService.GetGrievanceCountByDepartmentAsync();

        return Ok(statistics);
    }

    /// <summary>
    /// Get grievance count by category (Admins and Officers)
    /// </summary>
    [HttpGet("statistics/by-category")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetGrievanceCountByCategory()
    {
        var statistics = await _grievanceService.GetGrievanceCountByCategoryAsync();

        return Ok(statistics);
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
