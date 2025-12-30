namespace eSamadhaan.Application.DTOs.Reports;

public class MonthlyGrievanceReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalGrievances { get; set; }
    public int ResolvedGrievances { get; set; }
    public int PendingGrievances { get; set; }
    public double ResolutionRate { get; set; }
    public Dictionary<string, int> GrievancesByDepartment { get; set; } = new();
    public Dictionary<string, int> GrievancesByCategory { get; set; } = new();
}
