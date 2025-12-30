namespace eSamadhaan.Application.Interfaces.Services;

public interface IResolutionService
{
    Task<int> CreateResolutionAsync(int grievanceId, int resolvedByOfficerId, string resolutionRemarks);
    Task<object?> GetResolutionByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<object>> GetResolutionsByOfficerIdAsync(int officerId);
    Task<IEnumerable<object>> GetResolutionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task UpdateResolutionAsync(int resolutionId, string resolutionRemarks);
    
    // LINQ-based analytics
    Task<double> GetAverageResolutionTimeAsync();
    Task<double> GetAverageResolutionTimeByDepartmentAsync(int departmentId);
    Task<double> GetAverageResolutionTimeByCategoryAsync(int categoryId);
    Task<Dictionary<int, int>> GetResolutionCountByOfficerAsync();
}
