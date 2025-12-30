using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;

namespace eSamadhaan.Application.Interfaces.Repositories;

public interface IGrievanceStatusHistoryRepository
{
    Task<GrievanceStatusHistory?> GetByIdAsync(int id);
    Task<IEnumerable<GrievanceStatusHistory>> GetByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<GrievanceStatusHistory>> GetByUserIdAsync(int userId);
    Task<GrievanceStatusHistory?> GetLatestByGrievanceIdAsync(int grievanceId);
    Task<GrievanceStatusHistory> CreateAsync(GrievanceStatusHistory history);
    
    // LINQ-friendly query methods
    IQueryable<GrievanceStatusHistory> GetQueryable();
    Task<IEnumerable<GrievanceStatusHistory>> GetStatusTransitionsAsync(GrievanceStatus fromStatus, GrievanceStatus toStatus);
}
