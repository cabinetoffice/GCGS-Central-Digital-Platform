using CO.CDP.ApplicationRegistry.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for UserApplicationAssignment entity.
/// </summary>
public class UserApplicationAssignmentConfiguration : IEntityTypeConfiguration<UserApplicationAssignment>
{
    public void Configure(EntityTypeBuilder<UserApplicationAssignment> builder)
    {
        builder.ToTable("user_application_assignments");

        builder.HasKey(ua => ua.Id);
        builder.Property(ua => ua.Id).HasColumnName("id");

        builder.Property(ua => ua.UserOrganisationMembershipId)
            .HasColumnName("user_organisation_membership_id")
            .IsRequired();

        builder.Property(ua => ua.OrganisationApplicationId)
            .HasColumnName("organisation_application_id")
            .IsRequired();

        builder.HasIndex(ua => new { ua.UserOrganisationMembershipId, ua.OrganisationApplicationId })
            .IsUnique()
            .HasDatabaseName("ix_user_app_assignments_membership_app");

        builder.Property(ua => ua.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(ua => ua.AssignedAt)
            .HasColumnName("assigned_at");

        builder.Property(ua => ua.AssignedBy)
            .HasColumnName("assigned_by")
            .HasMaxLength(255);

        builder.Property(ua => ua.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(ua => ua.RevokedBy)
            .HasColumnName("revoked_by")
            .HasMaxLength(255);

        // Soft delete properties
        builder.Property(ua => ua.IsDeleted).HasColumnName("is_deleted");
        builder.Property(ua => ua.DeletedAt).HasColumnName("deleted_at");
        builder.Property(ua => ua.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(ua => ua.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(ua => ua.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(ua => ua.ModifiedAt).HasColumnName("modified_at");
        builder.Property(ua => ua.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);
    }
}
