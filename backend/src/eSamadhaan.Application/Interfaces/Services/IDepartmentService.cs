using eSamadhaan.Application.DTOs.Department;

namespace eSamadhaan.Application.Interfaces.Services;

public interface IDepartmentService
{
    Task<int> CreateDepartmentAsync(string name, string description);
    Task<object?> GetDepartmentByIdAsync(int id);
    Task<IEnumerable<object>> GetAllDepartmentsAsync();
    Task UpdateDepartmentAsync(int id, string name, string description);
    Task DeleteDepartmentAsync(int id);
    Task<bool> DepartmentExistsAsync(int id);
}
