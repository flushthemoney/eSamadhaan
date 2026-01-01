using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Interfaces.Repositories;

public interface IGrievanceCategoryRepository
{
    Task<GrievanceCategory?> GetByIdAsync(int id);
    Task<IEnumerable<GrievanceCategory>> GetAllAsync();
    Task<IEnumerable<GrievanceCategory>> GetByDepartmentIdAsync(int departmentId);
    Task<GrievanceCategory> CreateAsync(GrievanceCategory category);
    Task UpdateAsync(GrievanceCategory category);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
