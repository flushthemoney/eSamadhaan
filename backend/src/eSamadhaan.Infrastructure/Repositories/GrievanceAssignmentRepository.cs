using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eSamadhaan.Infrastructure.Repositories;

public class GrievanceAssignmentRepository : IGrievanceAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public GrievanceAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GrievanceAssignment?> GetByIdAsync(int id)
    {
        return await _context.GrievanceAssignments
            .Include(ga => ga.Grievance)
            .Include(ga => ga.Officer)
            .FirstOrDefaultAsync(ga => ga.Id == id);
    }

    public async Task<IEnumerable<GrievanceAssignment>> GetByGrievanceIdAsync(int grievanceId)
    {
        return await _context.GrievanceAssignments
            .Include(ga => ga.Officer)
            .Where(ga => ga.GrievanceId == grievanceId)
            .ToListAsync();
    }

    public async Task<GrievanceAssignment?> GetActiveByGrievanceIdAsync(int grievanceId)
    {
        return await _context.GrievanceAssignments
            .Include(ga => ga.Officer)
            .Where(ga => ga.GrievanceId == grievanceId && ga.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<GrievanceAssignment>> GetByOfficerIdAsync(int officerId)
    {
        return await _context.GrievanceAssignments
            .Include(ga => ga.Grievance)
                .ThenInclude(g => g.Category)
            .Include(ga => ga.Grievance)
                .ThenInclude(g => g.Department)
            .Where(ga => ga.OfficerId == officerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<GrievanceAssignment>> GetActiveByOfficerIdAsync(int officerId)
    {
        return await _context.GrievanceAssignments
            .Include(ga => ga.Grievance)
                .ThenInclude(g => g.Category)
            .Include(ga => ga.Grievance)
                .ThenInclude(g => g.Department)
            .Where(ga => ga.OfficerId == officerId && ga.IsActive)
            .ToListAsync();
    }

    public async Task<GrievanceAssignment> CreateAsync(GrievanceAssignment assignment)
    {
        _context.GrievanceAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAsync(GrievanceAssignment assignment)
    {
        _context.GrievanceAssignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var assignment = await _context.GrievanceAssignments.FindAsync(id);
        if (assignment != null)
        {
            _context.GrievanceAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }

    public IQueryable<GrievanceAssignment> GetQueryable()
    {
        return _context.GrievanceAssignments
            .Include(ga => ga.Grievance)
                .ThenInclude(g => g.Category)
            .Include(ga => ga.Grievance)
                .ThenInclude(g => g.Department)
            .Include(ga => ga.Officer)
            .AsQueryable();
    }
}
