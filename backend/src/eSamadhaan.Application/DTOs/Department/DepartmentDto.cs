namespace eSamadhaan.Application.DTOs.Department;

public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OfficerCount { get; set; }
    public int ActiveGrievanceCount { get; set; }
}
