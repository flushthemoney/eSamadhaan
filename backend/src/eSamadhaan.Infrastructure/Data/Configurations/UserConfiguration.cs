using eSamadhaan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eSamadhaan.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.DepartmentId)
            .IsRequired(false);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.DepartmentId)
            .HasDatabaseName("IX_Users_DepartmentId");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        // Relationships
        builder.HasOne(u => u.Department)
            .WithMany(d => d.Officers)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.SubmittedGrievances)
            .WithOne(g => g.Citizen)
            .HasForeignKey(g => g.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Assignments)
            .WithOne(a => a.Officer)
            .HasForeignKey(a => a.OfficerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.StatusChanges)
            .WithOne(sh => sh.ChangedByUser)
            .HasForeignKey(sh => sh.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Resolutions)
            .WithOne(r => r.ResolvedByOfficer)
            .HasForeignKey(r => r.ResolvedByOfficerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
