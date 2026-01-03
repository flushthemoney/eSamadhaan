using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;
using eSamadhaan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eSamadhaan.Infrastructure.Repositories;

public class GrievanceRepository : IGrievanceRepository
{
    private readonly ApplicationDbContext _context;

    public GrievanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Grievance?> GetByIdAsync(int id)
    {
        return await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Category)
            .Include(g => g.Department)
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.Officer!)
            .Include(g => g.Resolution)
                .ThenInclude(r => r!.ResolvedByOfficer)
            .Include(g => g.Feedback)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Grievance?> GetByGrievanceNumberAsync(string grievanceNumber)
    {
        return await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Category)
            .Include(g => g.Department)
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.Officer!)
            .Include(g => g.Resolution)
                .ThenInclude(r => r!.ResolvedByOfficer)
            .Include(g => g.Feedback)
            .FirstOrDefaultAsync(g => g.GrievanceNumber == grievanceNumber);
    }

    public async Task<IEnumerable<Grievance>> GetAllAsync()
    {
        return await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Category)
            .Include(g => g.Department)
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.Officer!)
            .Include(g => g.Resolution)
                .ThenInclude(r => r!.ResolvedByOfficer)
            .Include(g => g.Feedback)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grievance>> GetByCitizenIdAsync(int citizenId)
    {
        return await _context.Grievances
            .Include(g => g.Category)
            .Include(g => g.Department)
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.Officer!)
            .Include(g => g.Resolution)
                .ThenInclude(r => r!.ResolvedByOfficer)
            .Include(g => g.Feedback)
            .Where(g => g.CitizenId == citizenId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grievance>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Department)
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.Officer!)
            .Include(g => g.Resolution)
                .ThenInclude(r => r!.ResolvedByOfficer)
            .Include(g => g.Feedback)
            .Where(g => g.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grievance>> GetByDepartmentIdAsync(int departmentId)
    {
        return await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Category)
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.Officer!)
            .Include(g => g.Resolution)
                .ThenInclude(r => r!.ResolvedByOfficer)
            .Include(g => g.Feedback)
            .Where(g => g.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grievance>> GetByStatusAsync(GrievanceStatus status)
    {
        return await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Category)
            .Include(g => g.Department)
            .Include(g => g.Assignments.Where(a => a.IsActive))
                .ThenInclude(a => a.Officer!)
            .Include(g => g.Resolution)
                .ThenInclude(r => r!.ResolvedByOfficer)
            .Include(g => g.Feedback)
            .Where(g => g.CurrentStatus == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grievance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Category)
            .Include(g => g.Department)
            .Where(g => g.CreatedAt >= startDate && g.CreatedAt <= endDate)
            .ToListAsync();
    }

    public async Task<Grievance> CreateAsync(Grievance grievance)
    {
        _context.Grievances.Add(grievance);
        await _context.SaveChangesAsync();
        return grievance;
    }

    public async Task UpdateAsync(Grievance grievance)
    {
        _context.Grievances.Update(grievance);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var grievance = await _context.Grievances.FindAsync(id);
        if (grievance != null)
        {
            _context.Grievances.Remove(grievance);
            await _context.SaveChangesAsync();
        }
    }

    public IQueryable<Grievance> GetQueryable()
    {
        return _context.Grievances
            .Include(g => g.Citizen)
            .Include(g => g.Category)
            .Include(g => g.Department)
            .AsQueryable();
    }

    public async Task<int> CountByStatusAsync(GrievanceStatus status)
    {
        return await _context.Grievances
            .Where(g => g.CurrentStatus == status)
            .CountAsync();
    }

    public async Task<int> CountByDepartmentAsync(int departmentId)
    {
        return await _context.Grievances
            .Where(g => g.DepartmentId == departmentId)
            .CountAsync();
    }

    public async Task<int> CountByCategoryAsync(int categoryId)
    {
        return await _context.Grievances
            .Where(g => g.CategoryId == categoryId)
            .CountAsync();
    }
}
