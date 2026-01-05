using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Assignment;

public class AssignmentResponseDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public GrievanceStatus CurrentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
