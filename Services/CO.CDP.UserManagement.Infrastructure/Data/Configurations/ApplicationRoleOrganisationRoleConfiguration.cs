using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

public class ApplicationRoleOrganisationRoleConfiguration : IEntityTypeConfiguration<ApplicationRoleOrganisationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleOrganisationRole> builder)
    {
        builder.ToTable("application_role_organisation_roles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrganisationRoleId).HasColumnName("organisation_role_id").IsRequired();
        builder.Property(x => x.ApplicationRoleId).HasColumnName("application_role_id").IsRequired();
        builder.HasIndex(x => new { x.OrganisationRoleId, x.ApplicationRoleId })
            .IsUnique()
            .HasDatabaseName("ix_application_role_organisation_roles_role_application_role");

        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        builder.HasOne(x => x.OrganisationRole)
            .WithMany()
            .HasForeignKey(x => x.OrganisationRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ApplicationRole)
            .WithMany()
            .HasForeignKey(x => x.ApplicationRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
