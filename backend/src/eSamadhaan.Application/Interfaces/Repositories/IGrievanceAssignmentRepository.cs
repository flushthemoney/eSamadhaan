using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Interfaces.Repositories;

public interface IGrievanceAssignmentRepository
{
    Task<GrievanceAssignment?> GetByIdAsync(int id);
    Task<IEnumerable<GrievanceAssignment>> GetByGrievanceIdAsync(int grievanceId);
    Task<GrievanceAssignment?> GetActiveByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<GrievanceAssignment>> GetByOfficerIdAsync(int officerId);
    Task<IEnumerable<GrievanceAssignment>> GetActiveByOfficerIdAsync(int officerId);
    Task<GrievanceAssignment> CreateAsync(GrievanceAssignment assignment);
    Task UpdateAsync(GrievanceAssignment assignment);
    Task DeleteAsync(int id);
    
    // LINQ-friendly query methods
    IQueryable<GrievanceAssignment> GetQueryable();
}
