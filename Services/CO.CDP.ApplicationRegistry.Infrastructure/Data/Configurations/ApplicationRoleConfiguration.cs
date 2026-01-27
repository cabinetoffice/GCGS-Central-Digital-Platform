using CO.CDP.ApplicationRegistry.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ApplicationRole entity.
/// </summary>
public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("application_roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(r => new { r.ApplicationId, r.Name })
            .IsUnique()
            .HasDatabaseName("ix_application_roles_app_name");

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Soft delete properties
        builder.Property(r => r.IsDeleted).HasColumnName("is_deleted");
        builder.Property(r => r.DeletedAt).HasColumnName("deleted_at");
        builder.Property(r => r.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(r => r.ModifiedAt).HasColumnName("modified_at");
        builder.Property(r => r.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Relationships
        builder.HasMany(r => r.UserAssignments)
            .WithMany(ua => ua.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "user_assignment_roles",
                j => j.HasOne<UserApplicationAssignment>().WithMany().HasForeignKey("user_assignment_id"),
                j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey("role_id"),
                j =>
                {
                    j.HasKey("user_assignment_id", "role_id");
                    j.ToTable("user_assignment_roles");
                });
    }
}
