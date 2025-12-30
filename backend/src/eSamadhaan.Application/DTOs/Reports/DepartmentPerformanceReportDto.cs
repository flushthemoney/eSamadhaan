namespace eSamadhaan.Application.DTOs.Reports;

public class DepartmentPerformanceReportDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalGrievances { get; set; }
    public int ResolvedGrievances { get; set; }
    public int PendingGrievances { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeInDays { get; set; }
    public double AverageFeedbackRating { get; set; }
    public int ActiveOfficerCount { get; set; }
}
