namespace eSamadhaan.Domain.Entities;

public class GrievanceResolution
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public int ResolvedByOfficerId { get; set; }
    public string ResolutionRemarks { get; set; } = string.Empty;
    public DateTime ResolvedAt { get; set; }

    // Navigation properties
    public Grievance Grievance { get; set; } = null!;
    public User ResolvedByOfficer { get; set; } = null!;
}
