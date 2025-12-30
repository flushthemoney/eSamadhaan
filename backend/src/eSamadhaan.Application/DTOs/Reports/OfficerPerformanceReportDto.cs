namespace eSamadhaan.Application.DTOs.Reports;

public class OfficerPerformanceReportDto
{
    public int OfficerId { get; set; }
    public string OfficerName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalAssignedGrievances { get; set; }
    public int ResolvedGrievances { get; set; }
    public int PendingGrievances { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeInDays { get; set; }
    public double AverageFeedbackRating { get; set; }
}
