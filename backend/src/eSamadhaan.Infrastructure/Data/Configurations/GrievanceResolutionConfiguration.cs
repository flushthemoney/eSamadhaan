using eSamadhaan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSamadhaan.Infrastructure.Data.Configurations;

public class GrievanceResolutionConfiguration : IEntityTypeConfiguration<GrievanceResolution>
{
    public void Configure(EntityTypeBuilder<GrievanceResolution> builder)
    {
        builder.ToTable("GrievanceResolutions");

        builder.HasKey(gr => gr.Id);

        builder.Property(gr => gr.Id)
            .ValueGeneratedOnAdd();

        builder.Property(gr => gr.GrievanceId)
            .IsRequired();

        builder.Property(gr => gr.ResolvedByOfficerId)
            .IsRequired();

        builder.Property(gr => gr.ResolutionRemarks)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(gr => gr.ResolvedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(gr => gr.GrievanceId)
            .IsUnique()
            .HasDatabaseName("IX_GrievanceResolutions_GrievanceId");

        builder.HasIndex(gr => gr.ResolvedByOfficerId)
            .HasDatabaseName("IX_GrievanceResolutions_ResolvedByOfficerId");

        builder.HasIndex(gr => gr.ResolvedAt)
            .HasDatabaseName("IX_GrievanceResolutions_ResolvedAt");

        // Relationships
        builder.HasOne(gr => gr.Grievance)
            .WithOne(g => g.Resolution)
            .HasForeignKey<GrievanceResolution>(gr => gr.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gr => gr.ResolvedByOfficer)
            .WithMany(u => u.Resolutions)
            .HasForeignKey(gr => gr.ResolvedByOfficerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
