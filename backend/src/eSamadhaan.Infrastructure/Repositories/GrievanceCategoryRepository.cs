using eSamadhaan.Application.Interfaces.Repositories;
using eSamadhaan.Domain.Entities;
using eSamadhaan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eSamadhaan.Infrastructure.Repositories;

public class GrievanceCategoryRepository : IGrievanceCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public GrievanceCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GrievanceCategory?> GetByIdAsync(int id)
    {
        return await _context.GrievanceCategories.FindAsync(id);
    }

    public async Task<IEnumerable<GrievanceCategory>> GetAllAsync()
    {
        return await _context.GrievanceCategories.ToListAsync();
    }

    public async Task<GrievanceCategory> CreateAsync(GrievanceCategory category)
    {
        _context.GrievanceCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(GrievanceCategory category)
    {
        _context.GrievanceCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.GrievanceCategories.FindAsync(id);
        if (category != null)
        {
            _context.GrievanceCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.GrievanceCategories.AnyAsync(c => c.Id == id);
    }
}
