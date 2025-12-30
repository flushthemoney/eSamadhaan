using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Grievance;

public class UpdateGrievanceRequestDto
{
    public int CategoryId { get; set; }
    public int DepartmentId { get; set; }
    public string Description { get; set; } = string.Empty;
}
