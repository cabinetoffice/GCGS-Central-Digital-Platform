using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for PendingOrganisationInvite entity.
/// </summary>
public class PendingOrganisationInviteConfiguration : IEntityTypeConfiguration<PendingOrganisationInvite>
{
    public void Configure(EntityTypeBuilder<PendingOrganisationInvite> builder)
    {
        builder.ToTable("pending_organisation_invites");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(i => i.OrganisationId)
            .HasColumnName("organisation_id")
            .IsRequired();

        builder.HasIndex(i => new { i.Email, i.OrganisationId })
            .IsUnique()
            .HasDatabaseName("ix_pending_org_invites_email_org");

        builder.Property(i => i.OrganisationRole)
            .HasColumnName("organisation_role")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(i => i.CdpPersonInviteGuid)
            .HasColumnName("cdp_person_invite_guid")
            .IsRequired();

        builder.HasIndex(i => i.CdpPersonInviteGuid)
            .HasDatabaseName("ix_pending_org_invites_cdp_person_invite_guid");

        builder.Property(i => i.InvitedBy)
            .HasColumnName("invited_by")
            .HasMaxLength(255);

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
    }
}
