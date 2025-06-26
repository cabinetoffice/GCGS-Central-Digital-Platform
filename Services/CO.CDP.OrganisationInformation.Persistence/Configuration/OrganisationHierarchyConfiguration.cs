using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CO.CDP.OrganisationInformation.Persistence.Configuration;

/// <summary>
/// Configuration for the OrganisationHierarchy entity to define how it's mapped to the database.
/// </summary>
public class OrganisationHierarchyConfiguration : IEntityTypeConfiguration<OrganisationHierarchy>
{
    public void Configure(EntityTypeBuilder<OrganisationHierarchy> builder)
    {
        builder.ToTable("organisation_hierarchies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(e => e.ParentId)
            .IsRequired();

        builder.Property(e => e.ChildId)
            .IsRequired();

        builder.Property(e => e.CreatedOn)
            .IsRequired();

        builder.Property(e => e.SupersededOn)
            .IsRequired(false);

        builder.Property(e => e.Roles)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<List<PartyRole>>(v, new JsonSerializerOptions()) ?? new List<PartyRole>())
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(e => new { e.ParentId, e.ChildId });
    }
}
