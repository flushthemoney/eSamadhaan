using eSamadhaan.Application.DTOs.Auth;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(int id);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(int departmentId);
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
    Task<IEnumerable<UserDto>> GetActiveUsersAsync();
    Task UpdateUserAsync(int id, string name, string email, string? role, int? departmentId);
    Task DeactivateUserAsync(int id);
    Task ActivateUserAsync(int id);
    Task<Dictionary<string, int>> GetUserCountByRoleAsync();
    Task<Dictionary<int, int>> GetOfficerCountByDepartmentAsync();
}
