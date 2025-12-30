using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Interfaces.Repositories;

public interface IGrievanceRepository
{
    Task<Grievance?> GetByIdAsync(int id);
    Task<Grievance?> GetByGrievanceNumberAsync(string grievanceNumber);
    Task<IEnumerable<Grievance>> GetAllAsync();
    Task<IEnumerable<Grievance>> GetByCitizenIdAsync(int citizenId);
    Task<IEnumerable<Grievance>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Grievance>> GetByDepartmentIdAsync(int departmentId);
    Task<IEnumerable<Grievance>> GetByStatusAsync(GrievanceStatus status);
    Task<IEnumerable<Grievance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Grievance> CreateAsync(Grievance grievance);
    Task UpdateAsync(Grievance grievance);
    Task DeleteAsync(int id);
    
    // LINQ-friendly query methods
    IQueryable<Grievance> GetQueryable();
    Task<int> CountByStatusAsync(GrievanceStatus status);
    Task<int> CountByDepartmentAsync(int departmentId);
    Task<int> CountByCategoryAsync(int categoryId);
}
