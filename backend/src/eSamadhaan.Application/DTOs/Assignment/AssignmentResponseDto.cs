namespace eSamadhaan.Application.DTOs.Assignment;

public class AssignmentResponseDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public int OfficerId { get; set; }
    public string OfficerName { get; set; } = string.Empty;
    public string OfficerEmail { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }
}
