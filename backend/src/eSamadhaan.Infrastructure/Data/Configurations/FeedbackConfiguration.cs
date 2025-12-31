using eSamadhaan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSamadhaan.Infrastructure.Data.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.GrievanceId)
            .IsRequired();

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.SubmittedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(f => f.GrievanceId)
            .IsUnique()
            .HasDatabaseName("IX_Feedbacks_GrievanceId");

        builder.HasIndex(f => f.Rating)
            .HasDatabaseName("IX_Feedbacks_Rating");

        builder.HasIndex(f => f.SubmittedAt)
            .HasDatabaseName("IX_Feedbacks_SubmittedAt");

        // Relationships
        builder.HasOne(f => f.Grievance)
            .WithOne(g => g.Feedback)
            .HasForeignKey<Feedback>(f => f.GrievanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
