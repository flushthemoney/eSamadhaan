using eSamadhaan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSamadhaan.Infrastructure.Data.Configurations;

public class GrievanceAssignmentConfiguration : IEntityTypeConfiguration<GrievanceAssignment>
{
    public void Configure(EntityTypeBuilder<GrievanceAssignment> builder)
    {
        builder.ToTable("GrievanceAssignments");

        builder.HasKey(ga => ga.Id);

        builder.Property(ga => ga.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ga => ga.GrievanceId)
            .IsRequired();

        builder.Property(ga => ga.OfficerId)
            .IsRequired();

        builder.Property(ga => ga.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(ga => ga.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(ga => ga.GrievanceId)
            .HasDatabaseName("IX_GrievanceAssignments_GrievanceId");

        builder.HasIndex(ga => ga.OfficerId)
            .HasDatabaseName("IX_GrievanceAssignments_OfficerId");

        builder.HasIndex(ga => new { ga.GrievanceId, ga.IsActive })
            .HasDatabaseName("IX_GrievanceAssignments_GrievanceId_IsActive");

        builder.HasIndex(ga => new { ga.OfficerId, ga.IsActive })
            .HasDatabaseName("IX_GrievanceAssignments_OfficerId_IsActive");

        // Relationships
        builder.HasOne(ga => ga.Grievance)
            .WithMany(g => g.Assignments)
            .HasForeignKey(ga => ga.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ga => ga.Officer)
            .WithMany(u => u.Assignments)
            .HasForeignKey(ga => ga.OfficerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
