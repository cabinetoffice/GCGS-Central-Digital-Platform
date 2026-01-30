using CO.CDP.ApplicationRegistry.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Application entity.
/// </summary>
public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("applications");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.ClientId)
            .HasColumnName("client_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(a => a.ClientId)
            .IsUnique()
            .HasDatabaseName("ix_applications_client_id");

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(a => a.Category)
            .HasColumnName("category")
            .HasMaxLength(50);

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Soft delete properties
        builder.Property(a => a.IsDeleted).HasColumnName("is_deleted");
        builder.Property(a => a.DeletedAt).HasColumnName("deleted_at");
        builder.Property(a => a.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(a => a.ModifiedAt).HasColumnName("modified_at");
        builder.Property(a => a.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Relationships
        builder.HasMany(a => a.Permissions)
            .WithOne(p => p.Application)
            .HasForeignKey(p => p.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Roles)
            .WithOne(r => r.Application)
            .HasForeignKey(r => r.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.OrganisationApplications)
            .WithOne(oa => oa.Application)
            .HasForeignKey(oa => oa.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
