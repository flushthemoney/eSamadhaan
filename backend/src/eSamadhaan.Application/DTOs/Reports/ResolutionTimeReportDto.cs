namespace eSamadhaan.Application.DTOs.Reports;

public class ResolutionTimeReportDto
{
    public double AverageResolutionTimeInDays { get; set; }
    public double MedianResolutionTimeInDays { get; set; }
    public double MinResolutionTimeInDays { get; set; }
    public double MaxResolutionTimeInDays { get; set; }
    public int TotalResolvedGrievances { get; set; }
    public Dictionary<string, double> ResolutionTimeByDepartment { get; set; } = new();
    public Dictionary<string, double> ResolutionTimeByCategory { get; set; } = new();
}
