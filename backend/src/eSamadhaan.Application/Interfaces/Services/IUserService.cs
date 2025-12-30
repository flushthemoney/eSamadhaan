using eSamadhaan.Application.DTOs.Auth;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IUserService
{
    Task<object?> GetUserByIdAsync(int id);
    Task<object?> GetUserByEmailAsync(string email);
    Task<IEnumerable<object>> GetAllUsersAsync();
    Task<IEnumerable<object>> GetUsersByDepartmentAsync(int departmentId);
    Task<IEnumerable<object>> GetUsersByRoleAsync(string role);
    Task<IEnumerable<object>> GetActiveUsersAsync();
    Task UpdateUserAsync(int id, string name, string email, string? role, int? departmentId);
    Task DeactivateUserAsync(int id);
    Task ActivateUserAsync(int id);
    Task<Dictionary<string, int>> GetUserCountByRoleAsync();
    Task<Dictionary<int, int>> GetOfficerCountByDepartmentAsync();
}
