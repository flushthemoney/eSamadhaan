using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;
using eSamadhaan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eSamadhaan.Infrastructure.Repositories;

public class GrievanceStatusHistoryRepository : IGrievanceStatusHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public GrievanceStatusHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GrievanceStatusHistory?> GetByIdAsync(int id)
    {
        return await _context.GrievanceStatusHistories
            .Include(h => h.Grievance)
            .Include(h => h.ChangedByUser)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<GrievanceStatusHistory>> GetByGrievanceIdAsync(int grievanceId)
    {
        return await _context.GrievanceStatusHistories
            .Include(h => h.ChangedByUser)
            .Where(h => h.GrievanceId == grievanceId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GrievanceStatusHistory>> GetByUserIdAsync(int userId)
    {
        return await _context.GrievanceStatusHistories
            .Include(h => h.Grievance)
            .Where(h => h.ChangedByUserId == userId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<GrievanceStatusHistory?> GetLatestByGrievanceIdAsync(int grievanceId)
    {
        return await _context.GrievanceStatusHistories
            .Include(h => h.ChangedByUser)
            .Where(h => h.GrievanceId == grievanceId)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<GrievanceStatusHistory> CreateAsync(GrievanceStatusHistory history)
    {
        _context.GrievanceStatusHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public IQueryable<GrievanceStatusHistory> GetQueryable()
    {
        return _context.GrievanceStatusHistories
            .Include(h => h.Grievance)
            .Include(h => h.ChangedByUser)
            .AsQueryable();
    }

    public async Task<IEnumerable<GrievanceStatusHistory>> GetStatusTransitionsAsync(GrievanceStatus fromStatus, GrievanceStatus toStatus)
    {
        return await _context.GrievanceStatusHistories
            .Include(h => h.Grievance)
            .Include(h => h.ChangedByUser)
            .Where(h => h.OldStatus == fromStatus && h.NewStatus == toStatus)
            .ToListAsync();
    }
}
