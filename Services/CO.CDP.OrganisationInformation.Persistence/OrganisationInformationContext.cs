using CO.CDP.EntityFrameworkCore.Timestamps;
using CO.CDP.OrganisationInformation.Persistence.EntityFrameworkCore;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;
using System.Text.Json.Serialization;
using CO.CDP.MQ.Outbox;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationInformationContext(DbContextOptions<OrganisationInformationContext> options)
    : DbContext(options), IOutboxMessageDbContext
{
    public DbSet<Announcement> Announcements { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Organisation> Organisations { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Form> Forms { get; set; } = null!;
    public DbSet<SharedConsent> SharedConsents { get; set; } = null!;
    public DbSet<SharedConsentConsortium> SharedConsentConsortiums { get; set; } = null!;
    public DbSet<ConnectedEntity> ConnectedEntities { get; set; } = null!;
    public DbSet<AuthenticationKey> AuthenticationKeys { get; set; } = null!;
    public DbSet<FormAnswerSet> FormAnswerSets { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PersonInvite> PersonInvites { get; set; } = null!;
    public DbSet<OrganisationJoinRequest> OrganisationJoinRequests { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<OrganisationParty> OrganisationParties { get; set; } = null!;
    public DbSet<MouSignature> MouSignature { get; set; } = null!;
    public DbSet<Mou> Mou { get; set; } = null!;
    public DbSet<MouEmailReminder> MouEmailReminders { get; set; } = null!;

    public DbSet<OrganisationAddressSnapshot> OrganisationAddressSnapshot { get; set; } = null!;
    public DbSet<IdentifierSnapshot> IdentifierSnapshot { get; set; } = null!;
    public DbSet<ContactPointSnapshot> ContactPointSnapshot { get; set; } = null!;
    public DbSet<SupplierInformationSnapshot> SupplierInformationSnapshot { get; set; } = null!;
    public DbSet<ConnectedEntitySnapshot> ConnectedEntitySnapshot { get; set; } = null!;
    public DbSet<OrganisationSnapshot> OrganisationSnapshot { get; set; } = null!;
    public DbSet<OrganisationHierarchy> OrganisationHierarchies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<ControlCondition>();
        modelBuilder.HasPostgresEnum<ConnectedEntityType>();
        modelBuilder.HasPostgresEnum<ConnectedPersonType>();
        modelBuilder.HasPostgresEnum<ConnectedEntityIndividualAndTrustCategoryType>();
        modelBuilder.HasPostgresEnum<ConnectedOrganisationCategory>();
        modelBuilder.HasPostgresEnum<OrganisationType>();
        modelBuilder.HasPostgresEnum<OrganisationRelationship>();

        modelBuilder.Entity<ConnectedEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.OwnsOne(e => e.Organisation, a =>
            {
                a.HasKey(e => e.Id);
                a.Property(ai => ai.Id).HasColumnName("connected_organisation_id");
                a.Property(z => z.CreatedOn).HasTimestampDefault();
                a.Property(z => z.UpdatedOn).HasTimestampDefault();
                a.ToTable("connected_organisation");
            });

            entity.OwnsOne(e => e.IndividualOrTrust, a =>
            {
                a.HasKey(e => e.Id);
                a.Property(ai => ai.Id).HasColumnName("connected_individual_trust_id");
                a.Property(z => z.CreatedOn).HasTimestampDefault();
                a.Property(z => z.UpdatedOn).HasTimestampDefault();
                a.ToTable("connected_individual_trust");
            });
        });

        modelBuilder.Entity<OrganisationHierarchy>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.RelationshipId).IsUnique();

            entity.HasIndex(e => e.ParentOrganisationId);

            entity.HasIndex(e => e.ChildOrganisationId);

            entity.HasIndex(e => new { e.ParentOrganisationId, e.ChildOrganisationId, e.SupersededOn })
                .HasFilter("superseded_on IS NULL");

            entity.ToTable("organisation_hierarchies");
        });

        modelBuilder.Entity<Identifier>().ToTable("identifiers");
        modelBuilder.Entity<ContactPoint>().ToTable("contact_points");
        modelBuilder.Entity<SupplierInformation>(a =>
        {
            a.OwnsOne(x => x.LegalForm, y =>
            {
                y.Property(z => z.CreatedOn).HasTimestampDefault();
                y.Property(z => z.UpdatedOn).HasTimestampDefault();
                y.ToTable("legal_forms");
            });
        });
        modelBuilder.Entity<OrganisationAddress>(a =>
        {
            a.HasKey(e => e.Id).HasName("pk_organisation_address");
        });

        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ReviewedBy);

            entity
                .HasMany(p => p.Persons)
                .WithMany(t => t.Organisations)
                .UsingEntity<OrganisationPerson>(j =>
                {
                    j.Property(op => op.Scopes).IsRequired()
                        .HasJsonColumn([], PropertyBuilderExtensions.ListComparer<string>());
                    j.Property(op => op.CreatedOn).HasTimestampDefault();
                    j.Property(op => op.UpdatedOn).HasTimestampDefault();
                });
        });

        modelBuilder.Entity<Address>()
            .ToTable("addresses");

        modelBuilder.Entity<AuthenticationKey>(e =>
        {
            e.HasIndex(e => new { e.Name, e.OrganisationId }).IsUnique().AreNullsDistinct(false);
            e.Property(e => e.Scopes).HasJsonColumn([], PropertyBuilderExtensions.ListComparer<string>());
        });

        modelBuilder.Entity<Person>()
            .HasMany(p => p.Tenants)
            .WithMany(t => t.Persons)
            .UsingEntity<TenantPerson>();

        modelBuilder.Entity<SupplierInformationSnapshot>(a =>
        {
            a.OwnsOne(x => x.LegalFormSnapshot, y => { y.ToTable("legal_forms_snapshot"); });
        });

        modelBuilder.Entity<ConnectedEntitySnapshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.OwnsMany(e => e.Addresses, a => { a.ToTable("connected_entity_address_snapshot"); });
            entity.OwnsOne(e => e.Organisation, a => { a.ToTable("connected_organisation_snapshot"); });
            entity.OwnsOne(e => e.IndividualOrTrust, a => { a.ToTable("connected_individual_trust_snapshot"); });
        });

        modelBuilder.Entity<OrganisationSnapshot>().ToTable("organisations_snapshot");
        modelBuilder.Entity<AddressSnapshot>().ToTable("addresses_snapshot");
        modelBuilder.Entity<ContactPointSnapshot>().ToTable("contact_points_snapshot");
        modelBuilder.Entity<IdentifierSnapshot>().ToTable("identifiers_snapshot");
        modelBuilder.Entity<ConnectedEntitySnapshot>().ToTable("connected_entities_snapshot");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityDate).IsAssignableFrom(entityType.ClrType) && !entityType.IsOwned())
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>("CreatedOn").HasTimestampDefault();
                modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>("UpdatedOn").HasTimestampDefault();
            }
        }

        OnFormModelCreating(modelBuilder);

        modelBuilder.OnOutboxMessageCreating();

        base.OnModelCreating(modelBuilder);
    }

    private static void OnFormModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Form>()
            .HasMany(e => e.Sections);

        modelBuilder.Entity<FormSection>(e =>
        {
            e.ToTable("form_sections");
            e.Property(p => p.Type).HasDefaultValue(FormSectionType.Standard);
            e.Property(p => p.Configuration)
                .IsRequired()
                .HasJsonColumn(new(), PropertyBuilderExtensions.RecordComparer<FormSectionConfiguration>());
        });

        modelBuilder.Entity<FormQuestion>(e =>
        {
            e.ToTable("form_questions");
            e.HasOne(fq => fq.NextQuestion);
            e.HasOne(fq => fq.NextQuestionAlternative);
            e.Property(p => p.Options)
                .IsRequired()
                .HasJsonColumn(new(),
                    PropertyBuilderExtensions.RecordComparer<FormQuestionOptions>(),
                    JsonOptions.SerializerOptions);
        });

        modelBuilder.Entity<FormAnswerSet>(e =>
        {
            e.ToTable("form_answer_sets");
            e.Property(p => p.Deleted)
                .IsRequired()
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<FormAnswer>(e =>
        {
            e.ToTable("form_answers");
            e.Property(p => p.AddressValue)
                .HasJsonColumn(PropertyBuilderExtensions.RecordComparer<FormAddress>(),
                    JsonOptions.SerializerOptions);

            e.Property(fa => fa.JsonValue)
                .HasColumnType("jsonb");
        });
    }

    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.Preserve
        };
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.AddInterceptors(new EntityDateInterceptor());
        optionsBuilder.ReplaceService<IHistoryRepository, CamelCaseHistoryContext>();
        base.OnConfiguring(optionsBuilder);
    }
}

internal static class PropertyBuilderExtensions
{
    public static ValueComparer<List<T>> ListComparer<T>() =>
        new(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)),
            c => c.ToList()
        );

    public static ValueComparer<T> RecordComparer<T>() where T : notnull =>
        new(
            (c1, c2) => c1 != null && c2 != null && c1.Equals(c2),
            c => c.GetHashCode(),
            c => c
        );

    public static PropertyBuilder<TProperty?> HasJsonColumn<TProperty>(
        this PropertyBuilder<TProperty?> propertyBuilder,
        ValueComparer<TProperty> valueComparer,
        JsonSerializerOptions? jsonSerializerOptions = null
    ) => propertyBuilder
        .HasColumnType("jsonb")
        .HasConversion(
            v => JsonSerializer.Serialize(v, jsonSerializerOptions ?? JsonSerializerOptions.Default),
            v => JsonSerializer.Deserialize<TProperty?>(v, jsonSerializerOptions ?? JsonSerializerOptions.Default),
            valueComparer
        );

    public static PropertyBuilder<TProperty> HasJsonColumn<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        TProperty defaultValue,
        ValueComparer<TProperty> valueComparer,
        JsonSerializerOptions? jsonSerializerOptions = null
    ) => propertyBuilder
        .HasColumnType("jsonb")
        .HasConversion(
            v => JsonSerializer.Serialize(v, jsonSerializerOptions ?? JsonSerializerOptions.Default),
            v => JsonSerializer.Deserialize<TProperty>(v, jsonSerializerOptions ?? JsonSerializerOptions.Default) ?? defaultValue,
            valueComparer
        );
}