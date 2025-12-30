namespace eSamadhaan.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<User> Officers { get; set; } = new List<User>();
    public ICollection<Grievance> Grievances { get; set; } = new List<Grievance>();
}
