namespace eSamadhaan.Application.DTOs.Reports;

public class CategorySummaryReportDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalGrievances { get; set; }
    public int ResolvedGrievances { get; set; }
    public int PendingGrievances { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeInDays { get; set; }
}
