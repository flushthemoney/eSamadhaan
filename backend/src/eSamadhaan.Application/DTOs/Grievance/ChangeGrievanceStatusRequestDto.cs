using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Grievance;

public class ChangeGrievanceStatusRequestDto
{
    public GrievanceStatus NewStatus { get; set; }
    public string? Remarks { get; set; }
}
