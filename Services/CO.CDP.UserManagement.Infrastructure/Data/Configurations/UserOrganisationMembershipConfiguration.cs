using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for UserOrganisationMembership entity.
/// </summary>
public class UserOrganisationMembershipConfiguration : IEntityTypeConfiguration<UserOrganisationMembership>
{
    public void Configure(EntityTypeBuilder<UserOrganisationMembership> builder)
    {
        builder.ToTable("user_organisation_memberships");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.UserPrincipalId)
            .HasColumnName("user_principal_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.CdpPersonId)
            .HasColumnName("cdp_person_id");

        builder.HasIndex(m => m.CdpPersonId)
            .HasDatabaseName("ix_user_org_memberships_cdp_person_id");

        builder.Property(m => m.OrganisationId)
            .HasColumnName("organisation_id")
            .IsRequired();

        builder.HasIndex(m => new { m.UserPrincipalId, m.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_user_org_memberships_user_org");

        builder.Property(m => m.OrganisationRole)
            .HasColumnName("organisation_role")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(m => m.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();

        builder.Property(m => m.InvitedBy)
            .HasColumnName("invited_by")
            .HasMaxLength(255);

        // Soft delete properties
        builder.Property(m => m.IsDeleted).HasColumnName("is_deleted");
        builder.Property(m => m.DeletedAt).HasColumnName("deleted_at");
        builder.Property(m => m.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(m => m.ModifiedAt).HasColumnName("modified_at");
        builder.Property(m => m.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Relationships
        builder.HasMany(m => m.ApplicationAssignments)
            .WithOne(a => a.UserOrganisationMembership)
            .HasForeignKey(a => a.UserOrganisationMembershipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
