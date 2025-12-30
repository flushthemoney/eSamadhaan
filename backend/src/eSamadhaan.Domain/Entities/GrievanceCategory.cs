namespace eSamadhaan.Domain.Entities;

public class GrievanceCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Grievance> Grievances { get; set; } = new List<Grievance>();
}
