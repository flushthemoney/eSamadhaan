using eSamadhaan.Application.DTOs.Grievance;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Services;

public class GrievanceService : IGrievanceService
{
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGrievanceCategoryRepository _categoryRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IGrievanceStatusHistoryRepository _statusHistoryRepository;
    private readonly IGrievanceAssignmentRepository _assignmentRepository;

    public GrievanceService(
        IGrievanceRepository grievanceRepository,
        IUserRepository userRepository,
        IGrievanceCategoryRepository categoryRepository,
        IDepartmentRepository departmentRepository,
        IGrievanceStatusHistoryRepository statusHistoryRepository,
        IGrievanceAssignmentRepository assignmentRepository)
    {
        _grievanceRepository = grievanceRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _departmentRepository = departmentRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<int> CreateGrievanceAsync(int citizenId, int categoryId, int departmentId, string description, string? attachmentUrl)
    {
        // Validate citizen exists
        var citizen = await _userRepository.GetByIdAsync(citizenId);
        if (citizen == null)
        {
            throw new NotFoundException("Citizen", citizenId);
        }

        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        // Validate department exists
        if (!await _departmentRepository.ExistsAsync(departmentId))
        {
            throw new NotFoundException("Department", departmentId);
        }

        // Validate category belongs to the selected department
        if (category.DepartmentId != departmentId)
        {
            throw new ValidationException($"Category '{category.Name}' does not belong to the selected department.");
        }

        // Validate description
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ValidationException("Grievance description is required.");
        }

        var grievance = new Grievance
        {
            GrievanceNumber = GenerateGrievanceNumber(),
            CitizenId = citizenId,
            CategoryId = categoryId,
            DepartmentId = departmentId,
            Description = description,
            AttachmentUrl = attachmentUrl,
            CurrentStatus = GrievanceStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdGrievance = await _grievanceRepository.CreateAsync(grievance);

        // Record initial status history
        await RecordStatusHistoryAsync(createdGrievance.Id, GrievanceStatus.Submitted, GrievanceStatus.Submitted, citizenId);

        return createdGrievance.Id;
    }

    public async Task<object> GetGrievanceByIdAsync(int id)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", id);
        }

        return MapToResponseDto(grievance);
    }

    public async Task<object> GetGrievanceByNumberAsync(string grievanceNumber)
    {
        var grievance = await _grievanceRepository.GetByGrievanceNumberAsync(grievanceNumber);
        if (grievance == null)
        {
            throw new NotFoundException($"Grievance with number '{grievanceNumber}' was not found.");
        }

        return MapToResponseDto(grievance);
    }

    public async Task<IEnumerable<object>> GetAllGrievancesAsync()
    {
        var grievances = await _grievanceRepository.GetAllAsync();
        return grievances.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetGrievancesByCitizenAsync(int citizenId, GrievanceStatus? status = null)
    {
        var grievances = await _grievanceRepository.GetByCitizenIdAsync(citizenId);
        
        // Apply status filter if provided
        if (status.HasValue)
        {
            grievances = grievances.Where(g => g.CurrentStatus == status.Value).ToList();
        }
        
        return grievances.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetGrievancesByDepartmentAsync(int departmentId)
    {
        var grievances = await _grievanceRepository.GetByDepartmentIdAsync(departmentId);
        return grievances.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetGrievancesByCategoryAsync(int categoryId)
    {
        var grievances = await _grievanceRepository.GetByCategoryIdAsync(categoryId);
        return grievances.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetGrievancesByStatusAsync(GrievanceStatus status)
    {
        var grievances = await _grievanceRepository.GetByStatusAsync(status);
        return grievances.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> SearchGrievancesAsync(
        string? grievanceNumber, 
        int? departmentId, 
        int? categoryId, 
        GrievanceStatus? status, 
        DateTime? fromDate, 
        DateTime? toDate)
    {
        var query = _grievanceRepository.GetQueryable();

        // Apply filters using LINQ
        if (!string.IsNullOrWhiteSpace(grievanceNumber))
        {
            query = query.Where(g => g.GrievanceNumber.Contains(grievanceNumber));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(g => g.DepartmentId == departmentId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(g => g.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(g => g.CurrentStatus == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(g => g.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(g => g.CreatedAt <= toDate.Value);
        }

        var results = query.ToList();
        return results.Select(MapToResponseDto);
    }

    public async Task ChangeGrievanceStatusAsync(int grievanceId, GrievanceStatus newStatus, int changedByUserId, string? remarks)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        var user = await _userRepository.GetByIdAsync(changedByUserId);
        if (user == null)
        {
            throw new NotFoundException("User", changedByUserId);
        }

        var oldStatus = grievance.CurrentStatus;

        // Validate status transition
        ValidateStatusTransition(oldStatus, newStatus);

        // Update grievance status
        grievance.CurrentStatus = newStatus;
        grievance.UpdatedAt = DateTime.UtcNow;
        await _grievanceRepository.UpdateAsync(grievance);

        // Record status history
        await RecordStatusHistoryAsync(grievanceId, oldStatus, newStatus, changedByUserId, remarks);
    }

    public async Task<IEnumerable<object>> GetGrievanceStatusHistoryAsync(int grievanceId)
    {
        var history = await _statusHistoryRepository.GetByGrievanceIdAsync(grievanceId);
        
        return history.Select(h => new GrievanceStatusHistoryDto
        {
            Id = h.Id,
            GrievanceId = h.GrievanceId,
            OldStatus = h.OldStatus,
            NewStatus = h.NewStatus,
            ChangedByUserId = h.ChangedByUserId,
            ChangedByUserName = h.ChangedByUser?.Name ?? "Unknown",
            ChangedAt = h.ChangedAt,
            Remarks = h.Remarks
        });
    }

    public async Task ReopenGrievanceAsync(int grievanceId, int userId, string reason)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Can only reopen Closed grievances
        if (grievance.CurrentStatus != GrievanceStatus.Closed)
        {
            throw new BusinessRuleViolationException($"Only Closed grievances can be reopened. Current status: {grievance.CurrentStatus}");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ValidationException("Reason for reopening is required.");
        }

        var oldStatus = grievance.CurrentStatus;
        grievance.CurrentStatus = GrievanceStatus.Submitted;
        grievance.UpdatedAt = DateTime.UtcNow;
        await _grievanceRepository.UpdateAsync(grievance);

        // Record status history
        await RecordStatusHistoryAsync(grievanceId, oldStatus, GrievanceStatus.Submitted, userId);
    }

    public async Task CloseGrievanceAsync(int grievanceId, int userId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Can only close Resolved grievances
        if (grievance.CurrentStatus != GrievanceStatus.Resolved)
        {
            throw new BusinessRuleViolationException($"Only Resolved grievances can be closed. Current status: {grievance.CurrentStatus}");
        }

        var oldStatus = grievance.CurrentStatus;
        grievance.CurrentStatus = GrievanceStatus.Closed;
        grievance.UpdatedAt = DateTime.UtcNow;
        await _grievanceRepository.UpdateAsync(grievance);

        // Record status history
        await RecordStatusHistoryAsync(grievanceId, oldStatus, GrievanceStatus.Closed, userId);
    }

    public async Task<Dictionary<GrievanceStatus, int>> GetGrievanceCountByStatusAsync()
    {
        var query = _grievanceRepository.GetQueryable();
        
        // LINQ aggregation: Group by status and count
        var result = query
            .GroupBy(g => g.CurrentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionary(x => x.Status, x => x.Count);

        // Ensure all statuses are represented
        foreach (GrievanceStatus status in Enum.GetValues(typeof(GrievanceStatus)))
        {
            if (!result.ContainsKey(status))
            {
                result[status] = 0;
            }
        }

        return result;
    }

    public async Task<Dictionary<int, int>> GetGrievanceCountByDepartmentAsync()
    {
        var query = _grievanceRepository.GetQueryable();
        
        // LINQ aggregation: Group by department and count
        return query
            .GroupBy(g => g.DepartmentId)
            .Select(group => new { DepartmentId = group.Key, Count = group.Count() })
            .ToDictionary(x => x.DepartmentId, x => x.Count);
    }

    public async Task<Dictionary<int, int>> GetGrievanceCountByCategoryAsync()
    {
        var query = _grievanceRepository.GetQueryable();
        
        // LINQ aggregation: Group by category and count
        return query
            .GroupBy(g => g.CategoryId)
            .Select(group => new { CategoryId = group.Key, Count = group.Count() })
            .ToDictionary(x => x.CategoryId, x => x.Count);
    }

    // Private helper methods

    private void ValidateStatusTransition(GrievanceStatus from, GrievanceStatus to)
    {
        // Define valid transitions according to workflow
        var validTransitions = new Dictionary<GrievanceStatus, List<GrievanceStatus>>
        {
            { GrievanceStatus.Submitted, new List<GrievanceStatus> { GrievanceStatus.Assigned } },
            { GrievanceStatus.Assigned, new List<GrievanceStatus> { GrievanceStatus.InReview, GrievanceStatus.Submitted } },
            { GrievanceStatus.InReview, new List<GrievanceStatus> { GrievanceStatus.Resolved, GrievanceStatus.Assigned } },
            { GrievanceStatus.Resolved, new List<GrievanceStatus> { GrievanceStatus.Closed, GrievanceStatus.InReview } },
            { GrievanceStatus.Closed, new List<GrievanceStatus>() } // No transitions from Closed (use Reopen)
        };

        if (!validTransitions[from].Contains(to))
        {
            throw new InvalidStatusTransitionException(from, to);
        }
    }

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

    private string GenerateGrievanceNumber()
    {
        // Generate unique grievance number: GRV-YYYYMMDD-XXXXX
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(10000, 99999);
        return $"GRV-{timestamp}-{random}";
    }

    private GrievanceResponseDto MapToResponseDto(Grievance grievance)
    {
        var dto = new GrievanceResponseDto
        {
            Id = grievance.Id,
            GrievanceNumber = grievance.GrievanceNumber,
            CitizenId = grievance.CitizenId,
            CategoryId = grievance.CategoryId,
            CategoryName = grievance.Category?.Name ?? string.Empty,
            DepartmentId = grievance.DepartmentId,
            DepartmentName = grievance.Department?.Name ?? string.Empty,
            CurrentStatus = grievance.CurrentStatus,
            Description = grievance.Description,
            AttachmentUrl = grievance.AttachmentUrl,
            CreatedAt = grievance.CreatedAt,
            UpdatedAt = grievance.UpdatedAt
        };

        // Map current assignment
        var activeAssignment = grievance.Assignments?.FirstOrDefault(a => a.IsActive);
        if (activeAssignment != null)
        {
            dto.CurrentAssignment = new AssignmentDto
            {
                OfficerId = activeAssignment.OfficerId,
                OfficerName = activeAssignment.Officer?.Name ?? string.Empty,
                AssignedAt = activeAssignment.AssignedAt
            };
        }

        // Map resolution
        if (grievance.Resolution != null)
        {
            dto.Resolution = new ResolutionDto
            {
                ResolvedByOfficerId = grievance.Resolution.ResolvedByOfficerId,
                ResolvedByOfficerName = grievance.Resolution.ResolvedByOfficer?.Name ?? string.Empty,
                ResolutionRemarks = grievance.Resolution.ResolutionRemarks,
                ResolvedAt = grievance.Resolution.ResolvedAt
            };
        }

        // Map feedback
        if (grievance.Feedback != null)
        {
            dto.Feedback = new FeedbackDto
            {
                Rating = grievance.Feedback.Rating,
                Comment = grievance.Feedback.Comment,
                SubmittedAt = grievance.Feedback.SubmittedAt
            };
        }

        return dto;
    }

    // ========================================
    // SUPERVISORY OFFICER SPECIFIC METHODS
    // ========================================

    public async Task<IEnumerable<object>> GetEscalatedGrievancesAsync(int escalationThresholdDays = 7)
    {
        var now = DateTime.UtcNow;

        // LINQ query to find escalated grievances
        var escalatedGrievances = _grievanceRepository.GetQueryable()
            .Where(g => 
                // Filter by IsEscalated flag
                g.IsEscalated)
            .OrderBy(g => g.EscalatedAt)
            .Select(g => new
            {
                Grievance = g,
                DaysSinceSubmission = (int)(now - g.CreatedAt).TotalDays,
                DaysSinceEscalation = g.EscalatedAt.HasValue ? (int)(now - g.EscalatedAt.Value).TotalDays : 0,
                ActiveAssignment = g.Assignments.FirstOrDefault(a => a.IsActive)
            })
            .ToList();

        // Map to DTO
        return escalatedGrievances.Select(item => new EscalatedGrievanceDto
        {
            Id = item.Grievance.Id,
            GrievanceNumber = item.Grievance.GrievanceNumber,
            CitizenName = item.Grievance.Citizen?.Name ?? string.Empty,
            CategoryName = item.Grievance.Category?.Name ?? string.Empty,
            DepartmentName = item.Grievance.Department?.Name ?? string.Empty,
            CurrentStatus = item.Grievance.CurrentStatus,
            CreatedAt = item.Grievance.CreatedAt,
            UpdatedAt = item.Grievance.UpdatedAt,
            DaysSinceSubmission = item.DaysSinceSubmission,
            DaysSinceLastUpdate = item.DaysSinceEscalation,
            AssignedOfficerName = item.ActiveAssignment?.Officer?.Name,
            HasSLABreach = item.DaysSinceSubmission > 15 // SLA default 15 days
        }).ToList();
    }

    public async Task<IEnumerable<object>> GetSLABreachedGrievancesAsync(int slaDays = 15)
    {
        var now = DateTime.UtcNow;
        var slaBreachDate = now.AddDays(-slaDays);

        // LINQ query to find grievances with SLA breach
        var breachedGrievances = _grievanceRepository.GetQueryable()
            .Where(g => 
                // Not yet resolved or closed
                (g.CurrentStatus != GrievanceStatus.Resolved && g.CurrentStatus != GrievanceStatus.Closed) &&
                // Created before SLA threshold
                g.CreatedAt <= slaBreachDate)
            .OrderBy(g => g.CreatedAt)
            .Select(g => new
            {
                Grievance = g,
                DaysSinceSubmission = (int)(now - g.CreatedAt).TotalDays,
                DaysSinceLastUpdate = (int)(now - g.UpdatedAt).TotalDays,
                ActiveAssignment = g.Assignments.FirstOrDefault(a => a.IsActive)
            })
            .ToList();

        // Map to DTO with severity calculation
        return breachedGrievances.Select(item =>
        {
            var breachByDays = item.DaysSinceSubmission - slaDays;
            var severityLevel = CalculateSeverityLevel(breachByDays);

            return new SLABreachDto
            {
                Id = item.Grievance.Id,
                GrievanceNumber = item.Grievance.GrievanceNumber,
                CitizenName = item.Grievance.Citizen?.Name ?? string.Empty,
                CategoryName = item.Grievance.Category?.Name ?? string.Empty,
                DepartmentName = item.Grievance.Department?.Name ?? string.Empty,
                CurrentStatus = item.Grievance.CurrentStatus,
                CreatedAt = item.Grievance.CreatedAt,
                UpdatedAt = item.Grievance.UpdatedAt,
                DaysSinceSubmission = item.DaysSinceSubmission,
                DaysSinceLastUpdate = item.DaysSinceLastUpdate,
                SLADays = slaDays,
                BreachByDays = breachByDays,
                AssignedOfficerName = item.ActiveAssignment?.Officer?.Name,
                SeverityLevel = severityLevel
            };
        }).ToList();
    }

    private string CalculateSeverityLevel(int breachByDays)
    {
        if (breachByDays <= 3) return "Low";
        if (breachByDays <= 7) return "Medium";
        if (breachByDays <= 14) return "High";
        return "Critical";
    }

    public async Task<bool> CanEscalateGrievanceAsync(int grievanceId, int citizenId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        
        if (grievance == null)
            return false;
        
        // Check if citizen owns the grievance
        if (grievance.CitizenId != citizenId)
            return false;
        
        // Check if already escalated
        if (grievance.IsEscalated)
            return false;
        
        // Check if minimum 7 days have elapsed
        var daysSinceCreation = (DateTime.UtcNow - grievance.CreatedAt).TotalDays;
        if (daysSinceCreation < 7)
            return false;
        
        return true;
    }

    public async Task EscalateGrievanceAsync(int grievanceId, int citizenId, string reason)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }
        
        // Check if citizen owns the grievance
        if (grievance.CitizenId != citizenId)
        {
            throw new UnauthorizedException("You can only escalate your own grievances.");
        }
        
        // Check if already escalated
        if (grievance.IsEscalated)
        {
            throw new BusinessRuleViolationException("Grievance has already been escalated.");
        }
        
        // Check if minimum 7 days have elapsed
        var daysSinceCreation = (DateTime.UtcNow - grievance.CreatedAt).TotalDays;
        if (daysSinceCreation < 7)
        {
            throw new BusinessRuleViolationException($"Grievance can only be escalated after 7 days. Current age: {Math.Floor(daysSinceCreation)} days.");
        }
        
        // Validate reason
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ValidationException("Escalation reason is required.");
        }
        
        // Escalate the grievance
        grievance.IsEscalated = true;
        grievance.EscalatedAt = DateTime.UtcNow;
        grievance.EscalationReason = reason;
        grievance.UpdatedAt = DateTime.UtcNow;
        
        await _grievanceRepository.UpdateAsync(grievance);
    }
}
