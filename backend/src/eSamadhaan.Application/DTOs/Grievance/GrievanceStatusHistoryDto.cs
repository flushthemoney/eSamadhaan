using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Grievance;

public class GrievanceStatusHistoryDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public GrievanceStatus OldStatus { get; set; }
    public GrievanceStatus NewStatus { get; set; }
    public int ChangedByUserId { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Remarks { get; set; }
}
