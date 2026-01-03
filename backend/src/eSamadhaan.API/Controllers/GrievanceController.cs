using eSamadhaan.Application.DTOs.Assignment;
using eSamadhaan.Application.DTOs.Feedback;
using eSamadhaan.Application.DTOs.Grievance;
using eSamadhaan.Application.DTOs.Resolution;
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
    private readonly IResolutionService _resolutionService;
    private readonly IFeedbackService _feedbackService;
    private readonly IReportService _reportService;

    public GrievanceController(
        IGrievanceService grievanceService,
        IAssignmentService assignmentService,
        IResolutionService resolutionService,
        IFeedbackService feedbackService,
        IReportService reportService)
    {
        _grievanceService = grievanceService;
        _assignmentService = assignmentService;
        _resolutionService = resolutionService;
        _feedbackService = feedbackService;
        _reportService = reportService;
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
    public async Task<IActionResult> GetMyGrievances([FromQuery] GrievanceStatus? status = null)
    {
        var citizenId = GetCurrentUserId();

        var grievances = await _grievanceService.GetGrievancesByCitizenAsync(citizenId, status);

        return Ok(grievances);
    }

    /// <summary>
    /// Get grievance queue for officers (assigned to them)
    /// </summary>
    [HttpGet("my-queue")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer")]
    public async Task<IActionResult> GetMyQueue(
        [FromQuery] GrievanceStatus? status = null,
        [FromQuery] int? categoryId = null)
    {
        var officerId = GetCurrentUserId();

        var assignments = await _assignmentService.GetActiveAssignmentsByOfficerIdAsync(officerId, status, categoryId);

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
    /// Get all grievances (SupervisoryOfficer and SystemAdmin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
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

    // ========================================
    // RESOLUTION ENDPOINTS
    // ========================================

    /// <summary>
    /// Submit resolution for a grievance (Officers only)
    /// </summary>
    [HttpPost("{grievanceId}/resolution")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer")]
    public async Task<IActionResult> SubmitResolution(
        int grievanceId,
        [FromBody] CreateResolutionRequestDto request)
    {
        var officerId = GetCurrentUserId();

        var resolutionId = await _resolutionService.CreateResolutionAsync(
            grievanceId,
            officerId,
            request.ResolutionRemarks);

        return CreatedAtAction(
            nameof(GetResolutionByGrievanceId),
            new { grievanceId },
            new { resolutionId, message = "Resolution submitted successfully" });
    }

    /// <summary>
    /// Get resolution for a specific grievance
    /// </summary>
    [HttpGet("{grievanceId}/resolution")]
    public async Task<IActionResult> GetResolutionByGrievanceId(int grievanceId)
    {
        // Apply authorization: Citizens can only view resolutions of their own grievances
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

        var resolution = await _resolutionService.GetResolutionByGrievanceIdAsync(grievanceId);

        if (resolution == null)
        {
            return NotFound(new { message = "Resolution not found for this grievance" });
        }

        return Ok(resolution);
    }

    // ========================================
    // FEEDBACK ENDPOINTS
    // ========================================

    /// <summary>
    /// Submit feedback for a resolved/closed grievance (Citizen only)
    /// </summary>
    [HttpPost("{grievanceId}/feedback")]
    [Authorize(Roles = "Citizen")]
    public async Task<IActionResult> SubmitFeedback(
        int grievanceId,
        [FromBody] CreateFeedbackRequestDto request)
    {
        // Verify the grievance belongs to the citizen
        var citizenId = GetCurrentUserId();
        var grievance = await _grievanceService.GetGrievanceByIdAsync(grievanceId);
        var grievanceDto = grievance as dynamic;
        
        if (grievanceDto?.CitizenId != citizenId)
        {
            return Forbid();
        }

        var feedbackId = await _feedbackService.CreateFeedbackAsync(
            grievanceId,
            request.Rating,
            request.Comment);

        return CreatedAtAction(
            nameof(GetFeedbackByGrievanceId),
            new { grievanceId },
            new { feedbackId, message = "Feedback submitted successfully" });
    }

    /// <summary>
    /// Get feedback for a specific grievance
    /// </summary>
    [HttpGet("{grievanceId}/feedback")]
    public async Task<IActionResult> GetFeedbackByGrievanceId(int grievanceId)
    {
        // Apply authorization: Citizens can only view feedback on their own grievances
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

        var feedback = await _feedbackService.GetFeedbackByGrievanceIdAsync(grievanceId);

        if (feedback == null)
        {
            return NotFound(new { message = "Feedback not found for this grievance" });
        }

        return Ok(feedback);
    }

    // ========================================
    // DASHBOARD & REPORT ENDPOINTS
    // ========================================

    /// <summary>
    /// Get dashboard summary with key metrics (Supervisor/Admin only)
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int? departmentId = null)
    {
        var userId = GetCurrentUserId();

        var dashboard = await _reportService.GetDashboardSummaryAsync(departmentId, userId);

        return Ok(dashboard);
    }

    /// <summary>
    /// Get department-scoped dashboard for department officers
    /// </summary>
    [HttpGet("dashboard/officer")]
    [Authorize(Roles = "DepartmentOfficer")]
    public async Task<IActionResult> GetOfficerDashboard()
    {
        var userId = GetCurrentUserId();

        var dashboard = await _reportService.GetOfficerDashboardSummaryAsync(userId);

        return Ok(dashboard);
    }

    /// <summary>
    /// Get grievance status report with counts and percentages
    /// </summary>
    [HttpGet("reports/status")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetGrievanceStatusReport()
    {
        var report = await _reportService.GetGrievanceStatusReportAsync();

        return Ok(report);
    }

    /// <summary>
    /// Get detailed list of grievances by status
    /// </summary>
    [HttpGet("reports/status/{status}")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetGrievancesByStatusDetailed(GrievanceStatus status)
    {
        var grievances = await _reportService.GetGrievancesByStatusDetailedAsync(status);

        return Ok(grievances);
    }

    /// <summary>
    /// Get department performance report
    /// </summary>
    [HttpGet("reports/department/{departmentId}/performance")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetDepartmentPerformanceReport(int departmentId)
    {
        var report = await _reportService.GetDepartmentPerformanceReportAsync(departmentId);

        return Ok(report);
    }

    /// <summary>
    /// Get performance reports for all departments
    /// </summary>
    [HttpGet("reports/departments/performance")]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetAllDepartmentPerformanceReports()
    {
        var reports = await _reportService.GetAllDepartmentPerformanceReportsAsync();

        return Ok(reports);
    }

    /// <summary>
    /// Get category summary report
    /// </summary>
    [HttpGet("reports/category/{categoryId}/summary")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetCategorySummaryReport(int categoryId)
    {
        var report = await _reportService.GetCategorySummaryReportAsync(categoryId);

        return Ok(report);
    }

    /// <summary>
    /// Get summary reports for all categories
    /// </summary>
    [HttpGet("reports/categories/summary")]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetAllCategorySummaryReports()
    {
        var reports = await _reportService.GetAllCategorySummaryReportsAsync();

        return Ok(reports);
    }

    /// <summary>
    /// Get overall resolution time report
    /// </summary>
    [HttpGet("reports/resolution-time")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetResolutionTimeReport()
    {
        var report = await _reportService.GetResolutionTimeReportAsync();

        return Ok(report);
    }

    /// <summary>
    /// Get resolution time report by department
    /// </summary>
    [HttpGet("reports/resolution-time/department/{departmentId}")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetResolutionTimeReportByDepartment(int departmentId)
    {
        var report = await _reportService.GetResolutionTimeReportByDepartmentAsync(departmentId);

        return Ok(report);
    }

    /// <summary>
    /// Get resolution time report by category
    /// </summary>
    [HttpGet("reports/resolution-time/category/{categoryId}")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetResolutionTimeReportByCategory(int categoryId)
    {
        var report = await _reportService.GetResolutionTimeReportByCategoryAsync(categoryId);

        return Ok(report);
    }

    /// <summary>
    /// Get officer performance report
    /// </summary>
    [HttpGet("reports/officer/{officerId}/performance")]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetOfficerPerformanceReport(int officerId)
    {
        var report = await _reportService.GetOfficerPerformanceReportAsync(officerId);

        return Ok(report);
    }

    /// <summary>
    /// Get top performing officers
    /// </summary>
    [HttpGet("reports/officers/top-performers")]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetTopPerformingOfficers([FromQuery] int topCount = 10)
    {
        var report = await _reportService.GetTopPerformingOfficersAsync(topCount);

        return Ok(report);
    }

    /// <summary>
    /// Get grievance trend report (daily, weekly, or monthly)
    /// </summary>
    [HttpGet("reports/trends")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetGrievanceTrendReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string groupBy = "day")
    {
        var report = await _reportService.GetGrievanceTrendReportAsync(startDate, endDate, groupBy);

        return Ok(report);
    }

    /// <summary>
    /// Get monthly grievance report
    /// </summary>
    [HttpGet("reports/monthly")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetMonthlyGrievanceReport(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var report = await _reportService.GetMonthlyGrievanceReportAsync(year, month);

        return Ok(report);
    }

    /// <summary>
    /// Get overall feedback analytics report
    /// </summary>
    [HttpGet("reports/feedback/analytics")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetFeedbackAnalyticsReport()
    {
        var report = await _reportService.GetFeedbackAnalyticsReportAsync();

        return Ok(report);
    }

    /// <summary>
    /// Get feedback analytics by department
    /// </summary>
    [HttpGet("reports/feedback/analytics/department/{departmentId}")]
    [Authorize(Roles = "DepartmentOfficer,SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetFeedbackAnalyticsByDepartment(int departmentId)
    {
        var report = await _reportService.GetFeedbackAnalyticsByDepartmentAsync(departmentId);

        return Ok(report);
    }

    // ========================================
    // SUPERVISORY OFFICER SPECIFIC ENDPOINTS
    // ========================================

    /// <summary>
    /// Get escalated grievances that require supervisory attention
    /// </summary>
    [HttpGet("escalated")]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetEscalatedGrievances()
    {
        var escalatedGrievances = await _grievanceService.GetEscalatedGrievancesAsync(7);

        return Ok(escalatedGrievances);
    }

    /// <summary>
    /// Get grievances with SLA breaches
    /// </summary>
    [HttpGet("sla-breaches")]
    [Authorize(Roles = "SupervisoryOfficer,SystemAdmin")]
    public async Task<IActionResult> GetSLABreachedGrievances([FromQuery] int slaDays = 15)
    {
        var slaBreach = await _grievanceService.GetSLABreachedGrievancesAsync(slaDays);

        return Ok(slaBreach);
    }

    /// <summary>
    /// Escalate a grievance (Citizen only)
    /// </summary>
    [HttpPost("{grievanceId}/escalate")]
    [Authorize(Roles = "Citizen")]
    public async Task<IActionResult> EscalateGrievance(
        int grievanceId,
        [FromBody] EscalateGrievanceRequestDto request)
    {
        var citizenId = GetCurrentUserId();

        await _grievanceService.EscalateGrievanceAsync(grievanceId, citizenId, request.Reason);

        return Ok(new { message = "Grievance escalated successfully" });
    }

    /// <summary>
    /// Check if a grievance can be escalated (Citizen only)
    /// </summary>
    [HttpGet("{grievanceId}/can-escalate")]
    [Authorize(Roles = "Citizen")]
    public async Task<IActionResult> CanEscalateGrievance(int grievanceId)
    {
        var citizenId = GetCurrentUserId();

        var canEscalate = await _grievanceService.CanEscalateGrievanceAsync(grievanceId, citizenId);

        return Ok(new { canEscalate });
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
