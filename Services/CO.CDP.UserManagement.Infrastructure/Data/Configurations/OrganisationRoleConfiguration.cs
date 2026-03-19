using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

public class OrganisationRoleConfiguration : IEntityTypeConfiguration<OrganisationRoleEntity>
{
    public void Configure(EntityTypeBuilder<OrganisationRoleEntity> builder)
    {
        builder.ToTable("organisation_roles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(x => x.SyncToOrganisationInformation)
            .HasColumnName("sync_to_organisation_information")
            .IsRequired();

        builder.Property(x => x.AutoAssignDefaultApplications)
            .HasColumnName("auto_assign_default_applications")
            .IsRequired();

        builder.Property(x => x.OrganisationInformationScopes)
            .HasColumnName("organisation_information_scopes")
            .HasColumnType("text")
            .HasDefaultValueSql("'[]'")
            .HasConversion(new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()))
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (left, right) => (left ?? new List<string>()).SequenceEqual(right ?? new List<string>()),
                value => (value ?? new List<string>()).Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                value => (value ?? new List<string>()).ToList()));

        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);
    }
}
