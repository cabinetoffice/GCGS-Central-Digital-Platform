using CO.CDP.UserManagement.Core.Constants;
using CO.CDP.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace CO.CDP.UserManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ApplicationRole entity.
/// </summary>
public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("application_roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(r => new { r.ApplicationId, r.Name })
            .IsUnique()
            .HasDatabaseName("ix_application_roles_app_name");

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.RequiredPartyRoles)
            .HasColumnName("required_party_roles")
            .HasColumnType("integer[]")
            .HasConversion(new ValueConverter<List<PartyRole>, IEnumerable<int>>(
                v => v.Select(x => (int)x),
                v => v.Select(x => (PartyRole)x).ToList()))
            .Metadata.SetValueComparer(new ValueComparer<List<PartyRole>>(
                (left, right) => (left ?? new List<PartyRole>()).SequenceEqual(right ?? new List<PartyRole>()),
                value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                value => value.ToList()));

        builder.Property(r => r.OrganisationInformationScopes)
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

        builder.Property(r => r.SyncToOrganisationInformation)
            .HasColumnName("sync_to_organisation_information")
            .IsRequired()
            .HasDefaultValue(false);

        // Soft delete properties
        builder.Property(r => r.IsDeleted).HasColumnName("is_deleted");
        builder.Property(r => r.DeletedAt).HasColumnName("deleted_at");
        builder.Property(r => r.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

        // Audit properties
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.CreatedBy).HasColumnName("created_by").HasMaxLength(255).IsRequired();
        builder.Property(r => r.ModifiedAt).HasColumnName("modified_at");
        builder.Property(r => r.ModifiedBy).HasColumnName("modified_by").HasMaxLength(255);

        // Relationships
        builder.HasMany(r => r.UserAssignments)
            .WithMany(ua => ua.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "user_assignment_roles",
                j => j.HasOne<UserApplicationAssignment>().WithMany().HasForeignKey("user_assignment_id"),
                j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey("role_id"),
                j =>
                {
                    j.HasKey("user_assignment_id", "role_id");
                    j.ToTable("user_assignment_roles");
                });

    }
}
