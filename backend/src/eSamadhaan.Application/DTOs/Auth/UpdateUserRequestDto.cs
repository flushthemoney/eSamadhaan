namespace eSamadhaan.Application.DTOs.Auth;

public class UpdateUserRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public int? DepartmentId { get; set; }
}
