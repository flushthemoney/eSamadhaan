using eSamadhaan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSamadhaan.Infrastructure.Data.Configurations;

public class GrievanceCategoryConfiguration : IEntityTypeConfiguration<GrievanceCategory>
{
    public void Configure(EntityTypeBuilder<GrievanceCategory> builder)
    {
        builder.ToTable("GrievanceCategories");

        builder.HasKey(gc => gc.Id);

        builder.Property(gc => gc.Id)
            .ValueGeneratedOnAdd();

        builder.Property(gc => gc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(gc => gc.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(gc => gc.DepartmentId)
            .IsRequired();

        // Indexes
        builder.HasIndex(gc => gc.Name)
            .IsUnique()
            .HasDatabaseName("IX_GrievanceCategories_Name");

        builder.HasIndex(gc => gc.DepartmentId)
            .HasDatabaseName("IX_GrievanceCategories_DepartmentId");

        // Relationships
        builder.HasOne(gc => gc.Department)
            .WithMany(d => d.Categories)
            .HasForeignKey(gc => gc.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(gc => gc.Grievances)
            .WithOne(g => g.Category)
            .HasForeignKey(g => g.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
