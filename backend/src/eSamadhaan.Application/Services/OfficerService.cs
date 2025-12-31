using eSamadhaan.Application.DTOs.Officer;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Services;

public class OfficerService : IOfficerService
{
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGrievanceStatusHistoryRepository _statusHistoryRepository;
    private readonly IGrievanceAssignmentRepository _assignmentRepository;

    public OfficerService(
        IGrievanceRepository grievanceRepository,
        IUserRepository userRepository,
        IGrievanceStatusHistoryRepository statusHistoryRepository,
        IGrievanceAssignmentRepository assignmentRepository)
    {
        _grievanceRepository = grievanceRepository;
        _userRepository = userRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<IEnumerable<OfficerGrievanceListDto>> GetDepartmentGrievancesAsync(int officerId)
    {
        // Get officer and verify they belong to a department
        var officer = await _userRepository.GetByIdAsync(officerId);
        if (officer == null)
        {
            throw new NotFoundException("Officer", officerId);
        }

        if (!officer.DepartmentId.HasValue)
        {
            throw new BusinessRuleViolationException("Officer must be assigned to a department");
        }

        if (officer.Role != "DepartmentOfficer")
        {
            throw new UnauthorizedException("Only Department Officers can access this resource");
        }

        // Get all grievances for the officer's department
        var departmentGrievances = await _grievanceRepository.GetByDepartmentIdAsync(officer.DepartmentId.Value);

        // Get active assignments for this officer
        var officerAssignments = await _assignmentRepository.GetActiveByOfficerIdAsync(officerId);
        var assignedGrievanceIds = officerAssignments.Select(a => a.GrievanceId).ToHashSet();

        // Map to DTOs using LINQ
        var grievanceDtos = departmentGrievances
            .Select(g => new OfficerGrievanceListDto
            {
                Id = g.Id,
                GrievanceNumber = g.GrievanceNumber,
                CategoryName = g.Category.Name,
                CurrentStatus = g.CurrentStatus,
                Description = g.Description,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
                IsAssignedToMe = assignedGrievanceIds.Contains(g.Id)
            })
            .OrderByDescending(g => g.CreatedAt)
            .ToList();

        return grievanceDtos;
    }

    public async Task<OfficerGrievanceDetailDto> GetGrievanceDetailAsync(int grievanceId, int officerId)
    {
        // Get officer and verify department access
        var officer = await _userRepository.GetByIdAsync(officerId);
        if (officer == null)
        {
            throw new NotFoundException("Officer", officerId);
        }

        if (!officer.DepartmentId.HasValue)
        {
            throw new BusinessRuleViolationException("Officer must be assigned to a department");
        }

        if (officer.Role != "DepartmentOfficer")
        {
            throw new UnauthorizedException("Only Department Officers can access this resource");
        }

        // Get grievance with related data
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Verify the grievance belongs to officer's department
        if (grievance.DepartmentId != officer.DepartmentId.Value)
        {
            throw new UnauthorizedException("You can only access grievances from your department");
        }

        // Get status history
        var statusHistory = await _statusHistoryRepository.GetByGrievanceIdAsync(grievanceId);

        // Get current active assignment
        var currentAssignment = await _assignmentRepository.GetActiveByGrievanceIdAsync(grievanceId);

        // Map to DTO
        var dto = new OfficerGrievanceDetailDto
        {
            Id = grievance.Id,
            GrievanceNumber = grievance.GrievanceNumber,
            CategoryId = grievance.CategoryId,
            CategoryName = grievance.Category.Name,
            DepartmentId = grievance.DepartmentId,
            DepartmentName = grievance.Department.Name,
            CurrentStatus = grievance.CurrentStatus,
            Description = grievance.Description,
            AttachmentUrl = grievance.AttachmentUrl,
            CreatedAt = grievance.CreatedAt,
            UpdatedAt = grievance.UpdatedAt,
            CurrentAssignment = currentAssignment != null ? new OfficerAssignmentDto
            {
                OfficerId = currentAssignment.OfficerId,
                OfficerName = currentAssignment.Officer.Name,
                AssignedAt = currentAssignment.AssignedAt,
                IsActive = currentAssignment.IsActive
            } : null,
            StatusHistory = statusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OfficerStatusHistoryDto
                {
                    Id = h.Id,
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    ChangedByUserName = h.ChangedByUser.Name,
                    ChangedAt = h.ChangedAt,
                    Remarks = h.Remarks
                })
                .ToList()
        };

        return dto;
    }

    public async Task ChangeGrievanceStatusAsync(int grievanceId, int officerId, ChangeStatusRequestDto request)
    {
        // Get officer and verify department access
        var officer = await _userRepository.GetByIdAsync(officerId);
        if (officer == null)
        {
            throw new NotFoundException("Officer", officerId);
        }

        if (!officer.DepartmentId.HasValue)
        {
            throw new BusinessRuleViolationException("Officer must be assigned to a department");
        }

        if (officer.Role != "DepartmentOfficer")
        {
            throw new UnauthorizedException("Only Department Officers can change grievance status");
        }

        // Get grievance
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Verify the grievance belongs to officer's department
        if (grievance.DepartmentId != officer.DepartmentId.Value)
        {
            throw new UnauthorizedException("You can only modify grievances from your department");
        }

        // Validate status transition
        ValidateStatusTransition(grievance.CurrentStatus, request.NewStatus);

        // Create status history entry
        var statusHistory = new GrievanceStatusHistory
        {
            GrievanceId = grievanceId,
            OldStatus = grievance.CurrentStatus,
            NewStatus = request.NewStatus,
            ChangedByUserId = officerId,
            ChangedAt = DateTime.UtcNow,
            Remarks = request.Remarks
        };

        await _statusHistoryRepository.CreateAsync(statusHistory);

        // Update grievance status
        grievance.CurrentStatus = request.NewStatus;
        grievance.UpdatedAt = DateTime.UtcNow;
        await _grievanceRepository.UpdateAsync(grievance);
    }

    private void ValidateStatusTransition(GrievanceStatus currentStatus, GrievanceStatus newStatus)
    {
        // Define valid status transitions for officers
        var validTransitions = new Dictionary<GrievanceStatus, List<GrievanceStatus>>
        {
            { GrievanceStatus.Submitted, new List<GrievanceStatus> { GrievanceStatus.Assigned } },
            { GrievanceStatus.Assigned, new List<GrievanceStatus> { GrievanceStatus.InReview } },
            { GrievanceStatus.InReview, new List<GrievanceStatus> { GrievanceStatus.Resolved, GrievanceStatus.Assigned } },
            { GrievanceStatus.Resolved, new List<GrievanceStatus> { GrievanceStatus.Closed, GrievanceStatus.InReview } },
            { GrievanceStatus.Closed, new List<GrievanceStatus>() } // No transitions from Closed
        };

        if (!validTransitions.ContainsKey(currentStatus))
        {
            throw new InvalidStatusTransitionException($"Invalid current status: {currentStatus}");
        }

        if (!validTransitions[currentStatus].Contains(newStatus))
        {
            throw new InvalidStatusTransitionException(
                $"Cannot transition from {currentStatus} to {newStatus}. Valid transitions: {string.Join(", ", validTransitions[currentStatus])}");
        }
    }
}
