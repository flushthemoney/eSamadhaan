using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Domain.Entities;

public class GrievanceStatusHistory
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public GrievanceStatus OldStatus { get; set; }
    public GrievanceStatus NewStatus { get; set; }
    public int ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation properties
    public Grievance Grievance { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
