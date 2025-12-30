using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IReportService
{
    // Status-based reports
    Task<object> GetGrievanceStatusReportAsync();
    Task<IEnumerable<object>> GetGrievancesByStatusDetailedAsync(GrievanceStatus status);
    
    // Department performance reports
    Task<object> GetDepartmentPerformanceReportAsync(int departmentId);
    Task<IEnumerable<object>> GetAllDepartmentPerformanceReportsAsync();
    
    // Category summary reports
    Task<object> GetCategorySummaryReportAsync(int categoryId);
    Task<IEnumerable<object>> GetAllCategorySummaryReportsAsync();
    
    // Resolution time analytics
    Task<object> GetResolutionTimeReportAsync();
    Task<object> GetResolutionTimeReportByDepartmentAsync(int departmentId);
    Task<object> GetResolutionTimeReportByCategoryAsync(int categoryId);
    
    // Officer performance
    Task<object> GetOfficerPerformanceReportAsync(int officerId);
    Task<IEnumerable<object>> GetTopPerformingOfficersAsync(int topCount);
    
    // Time-based reports
    Task<IEnumerable<object>> GetGrievanceTrendReportAsync(DateTime startDate, DateTime endDate, string groupBy);
    Task<object> GetMonthlyGrievanceReportAsync(int year, int month);
    
    // Feedback analytics
    Task<object> GetFeedbackAnalyticsReportAsync();
    Task<object> GetFeedbackAnalyticsByDepartmentAsync(int departmentId);
    
    // Dashboard summary
    Task<object> GetDashboardSummaryAsync(int? departmentId, int? userId);
}
