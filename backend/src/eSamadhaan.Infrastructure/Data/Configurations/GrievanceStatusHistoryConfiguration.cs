using eSamadhaan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSamadhaan.Infrastructure.Data.Configurations;

public class GrievanceStatusHistoryConfiguration : IEntityTypeConfiguration<GrievanceStatusHistory>
{
    public void Configure(EntityTypeBuilder<GrievanceStatusHistory> builder)
    {
        builder.ToTable("GrievanceStatusHistories");

        builder.HasKey(gsh => gsh.Id);

        builder.Property(gsh => gsh.Id)
            .ValueGeneratedOnAdd();

        builder.Property(gsh => gsh.GrievanceId)
            .IsRequired();

        // Enum to int mappings
        builder.Property(gsh => gsh.OldStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(gsh => gsh.NewStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(gsh => gsh.ChangedByUserId)
            .IsRequired();

        builder.Property(gsh => gsh.ChangedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(gsh => gsh.Remarks)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(gsh => gsh.GrievanceId)
            .HasDatabaseName("IX_GrievanceStatusHistories_GrievanceId");

        builder.HasIndex(gsh => gsh.ChangedByUserId)
            .HasDatabaseName("IX_GrievanceStatusHistories_ChangedByUserId");

        builder.HasIndex(gsh => gsh.ChangedAt)
            .HasDatabaseName("IX_GrievanceStatusHistories_ChangedAt");

        builder.HasIndex(gsh => new { gsh.GrievanceId, gsh.ChangedAt })
            .HasDatabaseName("IX_GrievanceStatusHistories_GrievanceId_ChangedAt");

        // Relationships
        builder.HasOne(gsh => gsh.Grievance)
            .WithMany(g => g.StatusHistory)
            .HasForeignKey(gsh => gsh.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gsh => gsh.ChangedByUser)
            .WithMany(u => u.StatusChanges)
            .HasForeignKey(gsh => gsh.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
