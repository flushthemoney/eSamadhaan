namespace eSamadhaan.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Department? Department { get; set; }
    public ICollection<Grievance> SubmittedGrievances { get; set; } = new List<Grievance>();
    public ICollection<GrievanceAssignment> Assignments { get; set; } = new List<GrievanceAssignment>();
    public ICollection<GrievanceStatusHistory> StatusChanges { get; set; } = new List<GrievanceStatusHistory>();
    public ICollection<GrievanceResolution> Resolutions { get; set; } = new List<GrievanceResolution>();
}
