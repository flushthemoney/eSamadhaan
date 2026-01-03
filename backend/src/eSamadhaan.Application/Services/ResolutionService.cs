using eSamadhaan.Application.DTOs.Resolution;
using eSamadhaan.Application.Exceptions;
using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Application.Interfaces.Services;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Services;

public class ResolutionService : IResolutionService
{
    private readonly IGrievanceResolutionRepository _resolutionRepository;
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGrievanceStatusHistoryRepository _statusHistoryRepository;

    public ResolutionService(
        IGrievanceResolutionRepository resolutionRepository,
        IGrievanceRepository grievanceRepository,
        IUserRepository userRepository,
        IGrievanceStatusHistoryRepository statusHistoryRepository)
    {
        _resolutionRepository = resolutionRepository;
        _grievanceRepository = grievanceRepository;
        _userRepository = userRepository;
        _statusHistoryRepository = statusHistoryRepository;
    }

    public async Task<int> CreateResolutionAsync(int grievanceId, int resolvedByOfficerId, string resolutionRemarks)
    {
        // Validate grievance exists
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
        {
            throw new NotFoundException("Grievance", grievanceId);
        }

        // Validate officer exists
        var officer = await _userRepository.GetByIdAsync(resolvedByOfficerId);
        if (officer == null)
        {
            throw new NotFoundException("Officer", resolvedByOfficerId);
        }

        // Validate officer role
        if (officer.Role != "DepartmentOfficer" && officer.Role != "SupervisoryOfficer")
        {
            throw new ValidationException($"User with ID {resolvedByOfficerId} is not an officer.");
        }

        // Check if grievance is in correct status
        if (grievance.CurrentStatus != GrievanceStatus.InReview)
        {
            throw new BusinessRuleViolationException($"Grievance can only be resolved when in 'InReview' status. Current status: {grievance.CurrentStatus}");
        }

        // Check if resolution already exists
        var existingResolution = await _resolutionRepository.GetByGrievanceIdAsync(grievanceId);
        if (existingResolution != null)
        {
            throw new BusinessRuleViolationException("Resolution already exists for this grievance. Use update instead.");
        }

        // Validate resolution remarks
        if (string.IsNullOrWhiteSpace(resolutionRemarks))
        {
            throw new ValidationException("Resolution remarks are required.");
        }

        // Create resolution
        var resolution = new GrievanceResolution
        {
            GrievanceId = grievanceId,
            ResolvedByOfficerId = resolvedByOfficerId,
            ResolutionRemarks = resolutionRemarks,
            ResolvedAt = DateTime.UtcNow
        };

        var createdResolution = await _resolutionRepository.CreateAsync(resolution);

        // Update grievance status to Resolved
        var oldStatus = grievance.CurrentStatus;
        grievance.CurrentStatus = GrievanceStatus.Resolved;
        grievance.UpdatedAt = DateTime.UtcNow;
        await _grievanceRepository.UpdateAsync(grievance);

        // Record status history with resolution remarks
        await RecordStatusHistoryAsync(grievanceId, oldStatus, GrievanceStatus.Resolved, resolvedByOfficerId, resolutionRemarks);

        return createdResolution.Id;
    }

    public async Task<object?> GetResolutionByGrievanceIdAsync(int grievanceId)
    {
        var resolution = await _resolutionRepository.GetByGrievanceIdAsync(grievanceId);
        
        if (resolution == null)
        {
            return null;
        }

        return MapToResponseDto(resolution);
    }

    public async Task<IEnumerable<object>> GetResolutionsByOfficerIdAsync(int officerId)
    {
        var resolutions = await _resolutionRepository.GetByOfficerIdAsync(officerId);
        return resolutions.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<object>> GetResolutionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var resolutions = await _resolutionRepository.GetByDateRangeAsync(startDate, endDate);
        return resolutions.Select(MapToResponseDto);
    }

    public async Task UpdateResolutionAsync(int resolutionId, string resolutionRemarks)
    {
        var resolution = await _resolutionRepository.GetByIdAsync(resolutionId);
        if (resolution == null)
        {
            throw new NotFoundException("Resolution", resolutionId);
        }

        // Validate resolution remarks
        if (string.IsNullOrWhiteSpace(resolutionRemarks))
        {
            throw new ValidationException("Resolution remarks are required.");
        }

        resolution.ResolutionRemarks = resolutionRemarks;
        await _resolutionRepository.UpdateAsync(resolution);
    }

    public async Task<double> GetAverageResolutionTimeAsync()
    {
        var query = _resolutionRepository.GetQueryable();
        
        // LINQ aggregation: Calculate average resolution time across all grievances
        var resolutionTimes = query
            .Join(_grievanceRepository.GetQueryable(),
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => new
                {
                    ResolutionTime = (resolution.ResolvedAt - grievance.CreatedAt).TotalHours
                })
            .ToList();

        if (!resolutionTimes.Any())
        {
            return 0;
        }

        return resolutionTimes.Average(x => x.ResolutionTime);
    }

    public async Task<double> GetAverageResolutionTimeByDepartmentAsync(int departmentId)
    {
        var query = _resolutionRepository.GetQueryable();
        
        // LINQ aggregation: Calculate average resolution time for specific department
        var resolutionTimes = query
            .Join(_grievanceRepository.GetQueryable(),
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => new
                {
                    DepartmentId = grievance.DepartmentId,
                    ResolutionTime = (resolution.ResolvedAt - grievance.CreatedAt).TotalHours
                })
            .Where(x => x.DepartmentId == departmentId)
            .ToList();

        if (!resolutionTimes.Any())
        {
            return 0;
        }

        return resolutionTimes.Average(x => x.ResolutionTime);
    }

    public async Task<double> GetAverageResolutionTimeByCategoryAsync(int categoryId)
    {
        var query = _resolutionRepository.GetQueryable();
        
        // LINQ aggregation: Calculate average resolution time for specific category
        var resolutionTimes = query
            .Join(_grievanceRepository.GetQueryable(),
                resolution => resolution.GrievanceId,
                grievance => grievance.Id,
                (resolution, grievance) => new
                {
                    CategoryId = grievance.CategoryId,
                    ResolutionTime = (resolution.ResolvedAt - grievance.CreatedAt).TotalHours
                })
            .Where(x => x.CategoryId == categoryId)
            .ToList();

        if (!resolutionTimes.Any())
        {
            return 0;
        }

        return resolutionTimes.Average(x => x.ResolutionTime);
    }

    public async Task<Dictionary<int, int>> GetResolutionCountByOfficerAsync()
    {
        var query = _resolutionRepository.GetQueryable();
        
        // LINQ aggregation: Count resolutions grouped by officer
        return query
            .GroupBy(r => r.ResolvedByOfficerId)
            .Select(group => new { OfficerId = group.Key, Count = group.Count() })
            .ToDictionary(x => x.OfficerId, x => x.Count);
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

    private ResolutionResponseDto MapToResponseDto(GrievanceResolution resolution)
    {
        return new ResolutionResponseDto
        {
            Id = resolution.Id,
            GrievanceId = resolution.GrievanceId,
            GrievanceNumber = resolution.Grievance?.GrievanceNumber ?? string.Empty,
            ResolvedByOfficerId = resolution.ResolvedByOfficerId,
            ResolvedByOfficerName = resolution.ResolvedByOfficer?.Name ?? string.Empty,
            ResolutionRemarks = resolution.ResolutionRemarks,
            ResolvedAt = resolution.ResolvedAt
        };
    }
}
