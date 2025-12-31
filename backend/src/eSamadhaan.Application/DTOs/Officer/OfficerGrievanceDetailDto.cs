using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Officer;

public class OfficerGrievanceDetailDto
{
    public int Id { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public GrievanceStatus CurrentStatus { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Current assignment info
    public OfficerAssignmentDto? CurrentAssignment { get; set; }
    
    // Status history with remarks
    public List<OfficerStatusHistoryDto> StatusHistory { get; set; } = new();
}

public class OfficerAssignmentDto
{
    public int OfficerId { get; set; }
    public string OfficerName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }
}

public class OfficerStatusHistoryDto
{
    public int Id { get; set; }
    public GrievanceStatus OldStatus { get; set; }
    public GrievanceStatus NewStatus { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Remarks { get; set; }
}
