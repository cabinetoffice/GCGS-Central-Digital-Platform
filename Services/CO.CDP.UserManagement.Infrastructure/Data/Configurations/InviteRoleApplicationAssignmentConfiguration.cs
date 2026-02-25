using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for InviteRoleApplicationAssignment entity.
/// </summary>
public class InviteRoleApplicationAssignmentConfiguration : IEntityTypeConfiguration<InviteRoleApplicationAssignment>
{
    public void Configure(EntityTypeBuilder<InviteRoleApplicationAssignment> builder)
    {
        builder.ToTable("invite_role_application_assignments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.InviteRoleMappingId)
            .HasColumnName("invite_role_mapping_id")
            .IsRequired();

        builder.Property(a => a.OrganisationApplicationId)
            .HasColumnName("organisation_application_id")
            .IsRequired();

        builder.Property(a => a.ApplicationRoleId)
            .HasColumnName("application_role_id")
            .IsRequired();

        // Unique index to prevent duplicate role assignments for same invite+app
        builder.HasIndex(a => new { a.InviteRoleMappingId, a.OrganisationApplicationId, a.ApplicationRoleId })
            .IsUnique()
            .HasDatabaseName("ix_invite_role_app_assignments_mapping_app_role");

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
        builder.HasOne(a => a.InviteRoleMapping)
            .WithMany(i => i.ApplicationAssignments)
            .HasForeignKey(a => a.InviteRoleMappingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.OrganisationApplication)
            .WithMany()
            .HasForeignKey(a => a.OrganisationApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.ApplicationRole)
            .WithMany()
            .HasForeignKey(a => a.ApplicationRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
