using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eSamadhaan.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ApplicationDbContext _context;

    public FeedbackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await _context.Feedbacks
            .Include(f => f.Grievance)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Feedback?> GetByGrievanceIdAsync(int grievanceId)
    {
        return await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.GrievanceId == grievanceId);
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        return await _context.Feedbacks
            .Include(f => f.Grievance)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetByRatingAsync(int rating)
    {
        return await _context.Feedbacks
            .Include(f => f.Grievance)
            .Where(f => f.Rating == rating)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Feedbacks
            .Include(f => f.Grievance)
            .Where(f => f.SubmittedAt >= startDate && f.SubmittedAt <= endDate)
            .ToListAsync();
    }

    public async Task<Feedback> CreateAsync(Feedback feedback)
    {
        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        return feedback;
    }

    public async Task UpdateAsync(Feedback feedback)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Feedback> GetQueryable()
    {
        return _context.Feedbacks
            .Include(f => f.Grievance)
                .ThenInclude(g => g.Department)
            .AsQueryable();
    }

    public async Task<double> GetAverageRatingAsync()
    {
        var feedbacks = await _context.Feedbacks.ToListAsync();
        return feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0;
    }

    public async Task<double> GetAverageRatingByDepartmentAsync(int departmentId)
    {
        var feedbacks = await _context.Feedbacks
            .Include(f => f.Grievance)
            .Where(f => f.Grievance.DepartmentId == departmentId)
            .ToListAsync();
        
        return feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0;
    }
}
