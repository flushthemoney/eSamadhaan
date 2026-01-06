using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IReportService
{
    // Status-based reports
    Task<object> GetGrievanceStatusReportAsync();
    
    // Department performance reports
    Task<object> GetDepartmentPerformanceReportAsync(int departmentId);
    
    // Resolution time analytics
    Task<object> GetResolutionTimeReportAsync();
    Task<object> GetResolutionTimeReportByDepartmentAsync(int departmentId);
    Task<object> GetResolutionTimeReportByCategoryAsync(int categoryId);
    
    // Officer performance
    Task<IEnumerable<object>> GetTopPerformingOfficersAsync(int topCount, int? departmentId = null);
    
    // Feedback analytics
    Task<object> GetFeedbackAnalyticsReportAsync();
    Task<object> GetFeedbackAnalyticsByDepartmentAsync(int departmentId);
    
    // Dashboard summary
    Task<object> GetDashboardSummaryAsync(int? departmentId, int? userId);
    Task<object> GetOfficerDashboardSummaryAsync(int userId);
}
