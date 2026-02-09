using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Organisation entity.
/// </summary>
public class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.CdpOrganisationGuid)
            .HasColumnName("cdp_organisation_guid")
            .IsRequired();

        builder.HasIndex(o => o.CdpOrganisationGuid)
            .IsUnique()
            .HasDatabaseName("ix_organisations_cdp_guid");

        builder.Property(o => o.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.Slug)
            .HasColumnName("slug")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(o => o.Slug)
            .IsUnique()
            .HasDatabaseName("ix_organisations_slug");

        builder.Property(o => o.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Soft delete properties
        builder.Property(o => o.IsDeleted).HasColumnName("is_deleted");
        builder.Property(o => o.DeletedAt).HasColumnName("deleted_at");
        builder.Property(o => o.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(o => o.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(o => o.ModifiedAt).HasColumnName("modified_at");
        builder.Property(o => o.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Relationships
        builder.HasMany(o => o.UserMemberships)
            .WithOne(m => m.Organisation)
            .HasForeignKey(m => m.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.OrganisationApplications)
            .WithOne(oa => oa.Organisation)
            .HasForeignKey(oa => oa.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
