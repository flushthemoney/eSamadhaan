namespace eSamadhaan.Application.DTOs.Grievance;

public class CreateGrievanceRequestDto
{
    public int CategoryId { get; set; }
    public int DepartmentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}
