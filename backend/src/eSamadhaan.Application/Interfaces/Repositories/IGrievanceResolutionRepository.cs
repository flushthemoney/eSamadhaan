using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Interfaces.Repositories;

public interface IGrievanceResolutionRepository
{
    Task<GrievanceResolution?> GetByIdAsync(int id);
    Task<GrievanceResolution?> GetByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<GrievanceResolution>> GetByOfficerIdAsync(int officerId);
    Task<IEnumerable<GrievanceResolution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<GrievanceResolution> CreateAsync(GrievanceResolution resolution);
    Task UpdateAsync(GrievanceResolution resolution);
    
    // LINQ-friendly query methods
    IQueryable<GrievanceResolution> GetQueryable();
}
