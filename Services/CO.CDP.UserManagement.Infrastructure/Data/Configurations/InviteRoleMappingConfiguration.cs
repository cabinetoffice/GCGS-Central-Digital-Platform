using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for InviteRoleMapping entity.
/// </summary>
public class InviteRoleMappingConfiguration : IEntityTypeConfiguration<InviteRoleMapping>
{
    public void Configure(EntityTypeBuilder<InviteRoleMapping> builder)
    {
        builder.ToTable("invite_role_mappings");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.CdpPersonInviteGuid)
            .HasColumnName("cdp_person_invite_guid")
            .IsRequired();

        // Unique index on CdpPersonInviteGuid - ensures one mapping per CDP invite
        builder.HasIndex(i => i.CdpPersonInviteGuid)
            .IsUnique()
            .HasDatabaseName("ix_invite_role_mappings_cdp_person_invite_guid");

        builder.Property(i => i.OrganisationId)
            .HasColumnName("organisation_id")
            .IsRequired();

        builder.Property(i => i.OrganisationRole)
            .HasColumnName("organisation_role")
            .HasConversion<string>()
            .IsRequired();

        // Soft delete properties
        builder.Property(i => i.IsDeleted).HasColumnName("is_deleted");
        builder.Property(i => i.DeletedAt).HasColumnName("deleted_at");
        builder.Property(i => i.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(i => i.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(i => i.ModifiedAt).HasColumnName("modified_at");
        builder.Property(i => i.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Relationships
        builder.HasOne(i => i.Organisation)
            .WithMany()
            .HasForeignKey(i => i.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.ApplicationAssignments)
            .WithOne(a => a.InviteRoleMapping)
            .HasForeignKey(a => a.InviteRoleMappingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
