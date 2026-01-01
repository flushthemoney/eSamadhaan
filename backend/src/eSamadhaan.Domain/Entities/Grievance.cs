using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Domain.Entities;

public class Grievance
{
    public int Id { get; set; }
    public string GrievanceNumber { get; set; } = string.Empty;
    public int CitizenId { get; set; }
    public int CategoryId { get; set; }
    public int DepartmentId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public GrievanceStatus CurrentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Escalation properties
    public bool IsEscalated { get; set; } = false;
    public DateTime? EscalatedAt { get; set; }
    public string? EscalationReason { get; set; }

    // Navigation properties
    public User Citizen { get; set; } = null!;
    public GrievanceCategory Category { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public ICollection<GrievanceAssignment> Assignments { get; set; } = new List<GrievanceAssignment>();
    public ICollection<GrievanceStatusHistory> StatusHistory { get; set; } = new List<GrievanceStatusHistory>();
    public GrievanceResolution? Resolution { get; set; }
    public Feedback? Feedback { get; set; }
}
