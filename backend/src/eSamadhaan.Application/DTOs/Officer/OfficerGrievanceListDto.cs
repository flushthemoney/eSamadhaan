using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Officer;

public class OfficerGrievanceListDto
{
    public int Id { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public GrievanceStatus CurrentStatus { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsAssignedToMe { get; set; }
}
