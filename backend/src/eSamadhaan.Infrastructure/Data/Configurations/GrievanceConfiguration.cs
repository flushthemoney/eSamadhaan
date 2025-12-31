using eSamadhaan.Domain.Entities;
using eSamadhaan.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSamadhaan.Infrastructure.Data.Configurations;

public class GrievanceConfiguration : IEntityTypeConfiguration<Grievance>
{
    public void Configure(EntityTypeBuilder<Grievance> builder)
    {
        builder.ToTable("Grievances");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedOnAdd();

        builder.Property(g => g.GrievanceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(g => g.CitizenId)
            .IsRequired();

        builder.Property(g => g.CategoryId)
            .IsRequired();

        builder.Property(g => g.DepartmentId)
            .IsRequired();

        // Enum to int mapping - no database default, status set explicitly in application
        builder.Property(g => g.CurrentStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(g => g.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(g => g.GrievanceNumber)
            .IsUnique()
            .HasDatabaseName("IX_Grievances_GrievanceNumber");

        builder.HasIndex(g => g.CitizenId)
            .HasDatabaseName("IX_Grievances_CitizenId");

        builder.HasIndex(g => g.CategoryId)
            .HasDatabaseName("IX_Grievances_CategoryId");

        builder.HasIndex(g => g.DepartmentId)
            .HasDatabaseName("IX_Grievances_DepartmentId");

        builder.HasIndex(g => g.CurrentStatus)
            .HasDatabaseName("IX_Grievances_CurrentStatus");

        builder.HasIndex(g => g.CreatedAt)
            .HasDatabaseName("IX_Grievances_CreatedAt");

        builder.HasIndex(g => new { g.DepartmentId, g.CurrentStatus })
            .HasDatabaseName("IX_Grievances_DepartmentId_CurrentStatus");

        // Relationships
        builder.HasOne(g => g.Citizen)
            .WithMany(u => u.SubmittedGrievances)
            .HasForeignKey(g => g.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Category)
            .WithMany(c => c.Grievances)
            .HasForeignKey(g => g.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Department)
            .WithMany(d => d.Grievances)
            .HasForeignKey(g => g.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Assignments)
            .WithOne(a => a.Grievance)
            .HasForeignKey(a => a.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.StatusHistory)
            .WithOne(sh => sh.Grievance)
            .HasForeignKey(sh => sh.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Resolution)
            .WithOne(r => r.Grievance)
            .HasForeignKey<GrievanceResolution>(r => r.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Feedback)
            .WithOne(f => f.Grievance)
            .HasForeignKey<Feedback>(f => f.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
