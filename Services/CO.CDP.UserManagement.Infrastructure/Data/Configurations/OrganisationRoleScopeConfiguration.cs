using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

public class OrganisationRoleScopeConfiguration : IEntityTypeConfiguration<OrganisationRoleScope>
{
    public void Configure(EntityTypeBuilder<OrganisationRoleScope> builder)
    {
        builder.ToTable("organisation_role_scopes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OrganisationRoleId).HasColumnName("organisation_role_id").IsRequired();
        builder.Property(x => x.Source).HasColumnName("source").HasConversion<string>().IsRequired();
        builder.Property(x => x.ScopeName).HasColumnName("scope_name").HasMaxLength(255).IsRequired();
        builder.HasIndex(x => new { x.OrganisationRoleId, x.Source, x.ScopeName })
            .IsUnique()
            .HasDatabaseName("ix_organisation_role_scopes_role_source_scope");

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
    }
}
