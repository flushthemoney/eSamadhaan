using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eSamadhaan.Infrastructure.Repositories;

public class GrievanceResolutionRepository : IGrievanceResolutionRepository
{
    private readonly ApplicationDbContext _context;

    public GrievanceResolutionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GrievanceResolution?> GetByIdAsync(int id)
    {
        return await _context.GrievanceResolutions
            .Include(r => r.Grievance)
            .Include(r => r.ResolvedByOfficer)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<GrievanceResolution?> GetByGrievanceIdAsync(int grievanceId)
    {
        return await _context.GrievanceResolutions
            .Include(r => r.ResolvedByOfficer)
            .FirstOrDefaultAsync(r => r.GrievanceId == grievanceId);
    }

    public async Task<IEnumerable<GrievanceResolution>> GetByOfficerIdAsync(int officerId)
    {
        return await _context.GrievanceResolutions
            .Include(r => r.Grievance)
                .ThenInclude(g => g.Category)
            .Include(r => r.Grievance)
                .ThenInclude(g => g.Department)
            .Where(r => r.ResolvedByOfficerId == officerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<GrievanceResolution>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.GrievanceResolutions
            .Include(r => r.Grievance)
            .Include(r => r.ResolvedByOfficer)
            .Where(r => r.ResolvedAt >= startDate && r.ResolvedAt <= endDate)
            .ToListAsync();
    }

    public async Task<GrievanceResolution> CreateAsync(GrievanceResolution resolution)
    {
        _context.GrievanceResolutions.Add(resolution);
        await _context.SaveChangesAsync();
        return resolution;
    }

    public async Task UpdateAsync(GrievanceResolution resolution)
    {
        _context.GrievanceResolutions.Update(resolution);
        await _context.SaveChangesAsync();
    }

    public IQueryable<GrievanceResolution> GetQueryable()
    {
        return _context.GrievanceResolutions
            .Include(r => r.Grievance)
                .ThenInclude(g => g.Category)
            .Include(r => r.Grievance)
                .ThenInclude(g => g.Department)
            .Include(r => r.ResolvedByOfficer)
            .AsQueryable();
    }
}
