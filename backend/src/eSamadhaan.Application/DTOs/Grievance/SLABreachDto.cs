using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Grievance;

public class SLABreachDto
{
    public int Id { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public string CitizenName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public GrievanceStatus CurrentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int DaysSinceSubmission { get; set; }
    public int DaysSinceLastUpdate { get; set; }
    public int SLADays { get; set; }
    public int BreachByDays { get; set; }
    public string? AssignedOfficerName { get; set; }
    public string SeverityLevel { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
}
