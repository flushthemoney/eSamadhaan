namespace eSamadhaan.Domain.Entities;

public class GrievanceAssignment
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public int OfficerId { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public Grievance Grievance { get; set; } = null!;
    public User Officer { get; set; } = null!;
}
