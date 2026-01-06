using eSamadhaan.Application.DTOs.Officer;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IGrievanceService
{
    // Create and Retrieve
    Task<int> CreateGrievanceAsync(int citizenId, int categoryId, int departmentId, string description, string? attachmentUrl);
    Task<object> GetGrievanceByIdAsync(int id);
    Task<IEnumerable<object>> GetAllGrievancesAsync();
    Task<OfficerGrievanceDetailDto> GetGrievanceDetailForSupervisorAsync(int grievanceId);
    
    // Filter and Search
    Task<IEnumerable<object>> GetGrievancesByCitizenAsync(int citizenId, GrievanceStatus? status = null);
    Task<IEnumerable<object>> SearchGrievancesAsync(string? grievanceNumber, int? departmentId, int? categoryId, GrievanceStatus? status, DateTime? fromDate, DateTime? toDate);
    
    // Status Management
    Task ChangeGrievanceStatusAsync(int grievanceId, GrievanceStatus newStatus, int changedByUserId, string? remarks);
    Task<IEnumerable<object>> GetGrievanceStatusHistoryAsync(int grievanceId);
    
    // Lifecycle operations
    Task CloseGrievanceAsync(int grievanceId, int userId);
    
    // Supervisory Officer specific methods
    Task<IEnumerable<object>> GetEscalatedGrievancesAsync(int escalationThresholdDays = 7);
    
    // Citizen escalation
    Task EscalateGrievanceAsync(int grievanceId, int citizenId, string reason);
    Task<bool> CanEscalateGrievanceAsync(int grievanceId, int citizenId);
}
