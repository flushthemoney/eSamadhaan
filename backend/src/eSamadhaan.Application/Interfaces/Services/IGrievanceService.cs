using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IGrievanceService
{
    // Create and Retrieve
    Task<int> CreateGrievanceAsync(int citizenId, int categoryId, int departmentId, string description, string? attachmentUrl);
    Task<object> GetGrievanceByIdAsync(int id);
    Task<object> GetGrievanceByNumberAsync(string grievanceNumber);
    Task<IEnumerable<object>> GetAllGrievancesAsync();
    
    // Filter and Search
    Task<IEnumerable<object>> GetGrievancesByCitizenAsync(int citizenId);
    Task<IEnumerable<object>> GetGrievancesByDepartmentAsync(int departmentId);
    Task<IEnumerable<object>> GetGrievancesByCategoryAsync(int categoryId);
    Task<IEnumerable<object>> GetGrievancesByStatusAsync(GrievanceStatus status);
    Task<IEnumerable<object>> SearchGrievancesAsync(string? grievanceNumber, int? departmentId, int? categoryId, GrievanceStatus? status, DateTime? fromDate, DateTime? toDate);
    
    // Status Management
    Task ChangeGrievanceStatusAsync(int grievanceId, GrievanceStatus newStatus, int changedByUserId, string? remarks);
    Task<IEnumerable<object>> GetGrievanceStatusHistoryAsync(int grievanceId);
    
    // Lifecycle operations
    Task ReopenGrievanceAsync(int grievanceId, int userId, string reason);
    Task CloseGrievanceAsync(int grievanceId, int userId);
    
    // LINQ-based aggregations
    Task<Dictionary<GrievanceStatus, int>> GetGrievanceCountByStatusAsync();
    Task<Dictionary<int, int>> GetGrievanceCountByDepartmentAsync();
    Task<Dictionary<int, int>> GetGrievanceCountByCategoryAsync();
}
