using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllAsync();
    Task<Department> CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
