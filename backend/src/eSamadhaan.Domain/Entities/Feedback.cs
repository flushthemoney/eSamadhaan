namespace eSamadhaan.Domain.Entities;

public class Feedback
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    // Navigation properties
    public Grievance Grievance { get; set; } = null!;
}
