using CO.CDP.ApplicationRegistry.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ApplicationPermission entity.
/// </summary>
public class ApplicationPermissionConfiguration : IEntityTypeConfiguration<ApplicationPermission>
{
    public void Configure(EntityTypeBuilder<ApplicationPermission> builder)
    {
        builder.ToTable("application_permissions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(p => new { p.ApplicationId, p.Name })
            .IsUnique()
            .HasDatabaseName("ix_application_permissions_app_name");

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Soft delete properties
        builder.Property(p => p.IsDeleted).HasColumnName("is_deleted");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");
        builder.Property(p => p.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(p => p.ModifiedAt).HasColumnName("modified_at");
        builder.Property(p => p.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Many-to-many with ApplicationRole
        builder.HasMany(p => p.Roles)
            .WithMany(r => r.Permissions)
            .UsingEntity<Dictionary<string, object>>(
                "application_role_permissions",
                j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey("role_id"),
                j => j.HasOne<ApplicationPermission>().WithMany().HasForeignKey("permission_id"),
                j =>
                {
                    j.HasKey("role_id", "permission_id");
                    j.ToTable("application_role_permissions");
                });
    }
}
