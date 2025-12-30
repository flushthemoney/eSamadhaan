using eSamadhaan.Domain.Entities;

namespace eSamadhaan.Application.Interfaces.Repositories;

public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(int id);
    Task<Feedback?> GetByGrievanceIdAsync(int grievanceId);
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<IEnumerable<Feedback>> GetByRatingAsync(int rating);
    Task<IEnumerable<Feedback>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Feedback> CreateAsync(Feedback feedback);
    Task UpdateAsync(Feedback feedback);
    
    // LINQ-friendly query methods
    IQueryable<Feedback> GetQueryable();
    Task<double> GetAverageRatingAsync();
    Task<double> GetAverageRatingByDepartmentAsync(int departmentId);
}
