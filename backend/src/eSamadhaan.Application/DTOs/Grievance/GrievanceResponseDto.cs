using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.DTOs.Grievance;

public class GrievanceResponseDto
{
    public int Id { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public int CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string CitizenEmail { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public GrievanceStatus CurrentStatus { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Related data
    public AssignmentDto? CurrentAssignment { get; set; }
    public ResolutionDto? Resolution { get; set; }
    public FeedbackDto? Feedback { get; set; }
}

public class AssignmentDto
{
    public int OfficerId { get; set; }
    public string OfficerName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public class ResolutionDto
{
    public int ResolvedByOfficerId { get; set; }
    public string ResolvedByOfficerName { get; set; } = string.Empty;
    public string ResolutionRemarks { get; set; } = string.Empty;
    public DateTime ResolvedAt { get; set; }
}

public class FeedbackDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedAt { get; set; }
}
