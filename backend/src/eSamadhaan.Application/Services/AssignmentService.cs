using eSamadhaan.Application.DTOs.Assignment;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IGrievanceAssignmentRepository _assignmentRepository;
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGrievanceStatusHistoryRepository _statusHistoryRepository;

    public AssignmentService(
        IGrievanceAssignmentRepository assignmentRepository,
        IGrievanceRepository grievanceRepository,
        IUserRepository userRepository,
        IGrievanceStatusHistoryRepository statusHistoryRepository)
    {
        _assignmentRepository = assignmentRepository;
        _grievanceRepository = grievanceRepository;
        _userRepository = userRepository;
        _statusHistoryRepository = statusHistoryRepository;
    }

    public async Task<int> AssignGrievanceAsync(int grievanceId, int officerId, int assignedByUserId)
    {
        // Validate grievance exists
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Validate officer exists
        var officer = await _userRepository.GetByIdAsync(officerId);
        if (officer == null)
        {
            throw new NotFoundException("Officer", officerId);
        }

        // Validate officer role
        if (officer.Role != "DepartmentOfficer" && officer.Role != "SupervisoryOfficer")
        {
            throw new ValidationException($"User with ID {officerId} is not an officer.");
        }

        // Validate officer is active
        if (!officer.IsActive)
        {
            throw new ValidationException($"Officer with ID {officerId} is inactive.");
        }

        // Check if grievance is in correct status
        if (grievance.CurrentStatus != GrievanceStatus.Submitted)
        {
            throw new BusinessRuleViolationException($"Grievance can only be assigned when in 'Submitted' status. Current status: {grievance.CurrentStatus}");
        }

        // Check for existing active assignment
        var existingAssignment = await _assignmentRepository.GetActiveByGrievanceIdAsync(grievanceId);
        if (existingAssignment != null)
        {
            throw new BusinessRuleViolationException($"Grievance is already assigned to officer ID {existingAssignment.OfficerId}. Use reassignment instead.");
        }

        // Create assignment
        var assignment = new GrievanceAssignment
        {
            GrievanceId = grievanceId,
            OfficerId = officerId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdAssignment = await _assignmentRepository.CreateAsync(assignment);

        // Update grievance status to Assigned
        var oldStatus = grievance.CurrentStatus;
        grievance.CurrentStatus = GrievanceStatus.Assigned;
        grievance.UpdatedAt = DateTime.UtcNow;
        await _grievanceRepository.UpdateAsync(grievance);

        // Record status history with assignment remark
        var officerName = officer.Name ?? "Unknown Officer";
        var assignmentRemarks = $"Grievance assigned to {officerName}";
        await RecordStatusHistoryAsync(grievanceId, oldStatus, GrievanceStatus.Assigned, assignedByUserId, assignmentRemarks);

        return createdAssignment.Id;
    }

    public async Task ReassignGrievanceAsync(int grievanceId, int newOfficerId, int reassignedByUserId, string? reason)
    {
        // Validate grievance exists
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Validate new officer exists
        var newOfficer = await _userRepository.GetByIdAsync(newOfficerId);
        if (newOfficer == null)
        {
            throw new NotFoundException("Officer", newOfficerId);
        }

        // Validate officer role
        if (newOfficer.Role != "DepartmentOfficer" && newOfficer.Role != "SupervisoryOfficer")
        {
            throw new ValidationException($"User with ID {newOfficerId} is not an officer.");
        }

        // Validate officer is active
        if (!newOfficer.IsActive)
        {
            throw new ValidationException($"Officer with ID {newOfficerId} is inactive.");
        }

        // Get current active assignment
        var currentAssignment = await _assignmentRepository.GetActiveByGrievanceIdAsync(grievanceId);
        if (currentAssignment == null)
        {
            throw new BusinessRuleViolationException("No active assignment found for this grievance.");
        }

        // Validate not reassigning to same officer
        if (currentAssignment.OfficerId == newOfficerId)
        {
            throw new ValidationException("Cannot reassign to the same officer.");
        }

        // Capture current status before any updates
        var currentStatus = grievance.CurrentStatus;

        // Get old officer name for remarks
        var oldOfficer = await _userRepository.GetByIdAsync(currentAssignment.OfficerId);
        var oldOfficerName = oldOfficer?.Name ?? "Unknown";
        var newOfficerName = newOfficer.Name;

        // Deactivate current assignment
        currentAssignment.IsActive = false;
        await _assignmentRepository.UpdateAsync(currentAssignment);

        // Create new assignment
        var newAssignment = new GrievanceAssignment
        {
            GrievanceId = grievanceId,
            OfficerId = newOfficerId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _assignmentRepository.CreateAsync(newAssignment);

        // Update grievance timestamp
        grievance.UpdatedAt = DateTime.UtcNow;
        await _grievanceRepository.UpdateAsync(grievance);

        // Record status history for reassignment
        var reassignmentRemarks = $"Grievance reassigned from {oldOfficerName} to {newOfficerName}";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            reassignmentRemarks += $". Reason: {reason}";
        }
        await RecordStatusHistoryAsync(grievanceId, currentStatus, currentStatus, reassignedByUserId, reassignmentRemarks);
    }

    public async Task<object?> GetActiveAssignmentByGrievanceIdAsync(int grievanceId)
    {
        var assignment = await _assignmentRepository.GetActiveByGrievanceIdAsync(grievanceId);
        
        if (assignment == null)
        {
            return null;
        }

        return MapToResponseDto(assignment);
    }

    public async Task<IEnumerable<object>> GetAssignmentsByGrievanceIdAsync(int grievanceId)
    {
        var assignments = await _assignmentRepository.GetByGrievanceIdAsync(grievanceId);
        return assignments.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetAssignmentsByOfficerIdAsync(int officerId)
    {
        var assignments = await _assignmentRepository.GetByOfficerIdAsync(officerId);
        return assignments.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetActiveAssignmentsByOfficerIdAsync(int officerId, GrievanceStatus? status = null, int? categoryId = null)
    {
        var assignments = await _assignmentRepository.GetActiveByOfficerIdAsync(officerId);
        
        // Apply filters if provided
        var filteredAssignments = assignments.AsEnumerable();
        
        if (status.HasValue)
        {
            filteredAssignments = filteredAssignments.Where(a => a.Grievance.CurrentStatus == status.Value);
        }
        
        if (categoryId.HasValue)
        {
            filteredAssignments = filteredAssignments.Where(a => a.Grievance.CategoryId == categoryId.Value);
        }
        
        // Sort by most recent grievances first
        filteredAssignments = filteredAssignments.OrderByDescending(a => a.Grievance.CreatedAt);
        
        return filteredAssignments.Select(MapToResponseDto);
    }

    public async Task DeactivateAssignmentAsync(int assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
        {
            throw new NotFoundException("Assignment", assignmentId);
        }

        if (!assignment.IsActive)
        {
            throw new BusinessRuleViolationException("Assignment is already inactive.");
        }

        assignment.IsActive = false;
        await _assignmentRepository.UpdateAsync(assignment);
    }

    public async Task<int> GetActiveAssignmentCountByOfficerAsync(int officerId)
    {
        var query = _assignmentRepository.GetQueryable();
        
        // LINQ query: Count active assignments for officer
        return query.Count(a => a.OfficerId == officerId && a.IsActive);
    }

    public async Task<Dictionary<int, int>> GetAssignmentCountByDepartmentAsync()
    {
        var query = _assignmentRepository.GetQueryable();
        
        // LINQ aggregation: Count active assignments grouped by department
        return query
            .Where(a => a.IsActive)
            .Join(_grievanceRepository.GetQueryable(),
                assignment => assignment.GrievanceId,
                grievance => grievance.Id,
                (assignment, grievance) => new { assignment, grievance })
            .GroupBy(x => x.grievance.DepartmentId)
            .Select(group => new { DepartmentId = group.Key, Count = group.Count() })
            .ToDictionary(x => x.DepartmentId, x => x.Count);
    }

    // Private helper methods

    private async Task RecordStatusHistoryAsync(int grievanceId, GrievanceStatus oldStatus, GrievanceStatus newStatus, int changedByUserId, string? remarks = null)
    {
        var history = new GrievanceStatusHistory
        {
            GrievanceId = grievanceId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow,
            Remarks = remarks
        };

        await _statusHistoryRepository.CreateAsync(history);
    }

    private AssignmentResponseDto MapToResponseDto(GrievanceAssignment assignment)
    {
        return new AssignmentResponseDto
        {
            Id = assignment.Id,
            GrievanceId = assignment.GrievanceId,
            GrievanceNumber = assignment.Grievance?.GrievanceNumber ?? string.Empty,
            CategoryName = assignment.Grievance?.Category?.Name ?? string.Empty,
            CurrentStatus = assignment.Grievance?.CurrentStatus ?? GrievanceStatus.Submitted,
            CreatedAt = assignment.Grievance?.CreatedAt ?? DateTime.UtcNow
        };
    }
}
