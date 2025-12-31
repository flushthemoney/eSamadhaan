using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Officer;

public class ChangeStatusRequestDto
{
    public GrievanceStatus NewStatus { get; set; }
    public string? Remarks { get; set; }
}
