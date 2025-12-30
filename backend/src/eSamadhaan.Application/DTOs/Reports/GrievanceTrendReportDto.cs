namespace eSamadhaan.Application.DTOs.Reports;

public class GrievanceTrendReportDto
{
    public string Period { get; set; } = string.Empty;
    public int TotalGrievances { get; set; }
    public int SubmittedCount { get; set; }
    public int AssignedCount { get; set; }
    public int InReviewCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
}
