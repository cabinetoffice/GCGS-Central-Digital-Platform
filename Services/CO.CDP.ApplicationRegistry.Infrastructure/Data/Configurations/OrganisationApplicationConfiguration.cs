using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for OrganisationApplication entity.
/// </summary>
public class OrganisationApplicationConfiguration : IEntityTypeConfiguration<OrganisationApplication>
{
    public void Configure(EntityTypeBuilder<OrganisationApplication> builder)
    {
        builder.ToTable("organisation_applications");

        builder.HasKey(oa => oa.Id);
        builder.Property(oa => oa.Id).HasColumnName("id");

        builder.Property(oa => oa.OrganisationId)
            .HasColumnName("organisation_id")
            .IsRequired();

        builder.Property(oa => oa.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.HasIndex(oa => new { oa.OrganisationId, oa.ApplicationId })
            .IsUnique()
            .HasDatabaseName("ix_organisation_applications_org_app");

        builder.Property(oa => oa.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(oa => oa.EnabledAt)
            .HasColumnName("enabled_at");

        builder.Property(oa => oa.EnabledBy)
            .HasColumnName("enabled_by")
            .HasMaxLength(255);

        builder.Property(oa => oa.DisabledAt)
            .HasColumnName("disabled_at");

        builder.Property(oa => oa.DisabledBy)
            .HasColumnName("disabled_by")
            .HasMaxLength(255);

        // Soft delete properties
        builder.Property(oa => oa.IsDeleted).HasColumnName("is_deleted");
        builder.Property(oa => oa.DeletedAt).HasColumnName("deleted_at");
        builder.Property(oa => oa.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(oa => oa.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(oa => oa.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(oa => oa.ModifiedAt).HasColumnName("modified_at");
        builder.Property(oa => oa.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Relationships
        builder.HasMany(oa => oa.UserAssignments)
            .WithOne(ua => ua.OrganisationApplication)
            .HasForeignKey(ua => ua.OrganisationApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
