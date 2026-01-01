namespace eSamadhaan.Domain.Entities;

public class GrievanceCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DepartmentId { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<Grievance> Grievances { get; set; } = new List<Grievance>();
}
