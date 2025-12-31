using eSamadhaan.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eSamadhaan.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet definitions
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<GrievanceCategory> GrievanceCategories { get; set; } = null!;
    public DbSet<Grievance> Grievances { get; set; } = null!;
    public DbSet<GrievanceAssignment> GrievanceAssignments { get; set; } = null!;
    public DbSet<GrievanceStatusHistory> GrievanceStatusHistories { get; set; } = null!;
    public DbSet<GrievanceResolution> GrievanceResolutions { get; set; } = null!;
    public DbSet<Feedback> Feedbacks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
