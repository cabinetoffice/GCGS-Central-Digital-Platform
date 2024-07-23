using System.Text.Json;
using System.Text.Json.Serialization;
using CO.CDP.EntityFrameworkCore.Timestamps;
using CO.CDP.OrganisationInformation.Persistence.EntityFrameworkCore;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationInformationContext(DbContextOptions<OrganisationInformationContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Organisation> Organisations { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Form> Forms { get; set; } = null!;
    public DbSet<SharedConsent> SharedConsents { get; set; } = null!;
    public DbSet<ConnectedEntity> ConnectedEntities { get; set; } = null!;
    public DbSet<AuthenticationKey> AuthenticationKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<ControlCondition>();
        modelBuilder.HasPostgresEnum<ConnectedEntityType>();
        modelBuilder.HasPostgresEnum<ConnectedPersonType>();
        modelBuilder.HasPostgresEnum<ConnectedPersonCategory>();
        modelBuilder.HasPostgresEnum<ConnectedOrganisationCategory>();

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

        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.OwnsMany(e => e.Identifiers, a =>
            {
                a.HasKey(e => e.Id);
                a.Property(ai => ai.Primary);
                a.Property(ai => ai.IdentifierId);
                a.Property(ai => ai.Scheme);
                a.Property(ai => ai.LegalName);
                a.Property(ai => ai.Uri);
                a.Property(ai => ai.CreatedOn).HasTimestampDefault();
                a.Property(ai => ai.UpdatedOn).HasTimestampDefault();
                a.ToTable("identifiers");
            });

            entity.OwnsMany(e => e.ContactPoints, a =>
            {
                a.HasKey(e => e.Id);
                a.Property(ai => ai.Name);
                a.Property(ai => ai.Email);
                a.Property(ai => ai.Telephone);
                a.Property(ai => ai.Url);
                a.Property(ai => ai.CreatedOn).HasTimestampDefault();
                a.Property(ai => ai.UpdatedOn).HasTimestampDefault();
                a.ToTable("contact_points");
            });

            entity.OwnsOne(e => e.SupplierInfo, a =>
            {
                a.Property(z => z.CreatedOn).HasTimestampDefault();
                a.Property(z => z.UpdatedOn).HasTimestampDefault();
                a.ToTable("supplier_information");

                a.OwnsMany(x => x.Qualifications, y =>
                {
                    y.HasKey(z => z.Id);
                    y.Property(z => z.CreatedOn).HasTimestampDefault();
                    y.Property(z => z.UpdatedOn).HasTimestampDefault();
                    y.ToTable("qualifications");
                });

                a.OwnsMany(x => x.TradeAssurances, y =>
                {
                    y.HasKey(z => z.Id);
                    y.Property(z => z.CreatedOn).HasTimestampDefault();
                    y.Property(z => z.UpdatedOn).HasTimestampDefault();
                    y.ToTable("trade_assurances");
                });
                a.OwnsOne(x => x.LegalForm, y =>
                {
                    y.Property(z => z.CreatedOn).HasTimestampDefault();
                    y.Property(z => z.UpdatedOn).HasTimestampDefault();
                    y.ToTable("legal_forms");
                });
            });

            entity.OwnsOne(e => e.BuyerInfo, a =>
            {
                a.Property(z => z.CreatedOn).HasTimestampDefault();
                a.Property(z => z.UpdatedOn).HasTimestampDefault();
                a.ToTable("buyer_information");
            });

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

            entity.OwnsMany(e => e.Addresses, a =>
            {
                a.HasKey(e => e.Id);
            });
        });

        modelBuilder.Entity<Address>()
            .ToTable("addresses");

        modelBuilder.Entity<AuthenticationKey>(e =>
        {
            e.Property(e => e.Scopes).HasJsonColumn([], PropertyBuilderExtensions.ListComparer<string>());
        });

        modelBuilder.Entity<Person>()
            .HasMany(p => p.Tenants)
            .WithMany(t => t.Persons)
            .UsingEntity<TenantPerson>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityDate).IsAssignableFrom(entityType.ClrType) && !entityType.IsOwned())
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>("CreatedOn").HasTimestampDefault();
                modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>("UpdatedOn").HasTimestampDefault();
            }
        }

        OnFormModelCreating(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void OnFormModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Form>()
            .HasMany<FormSection>(e => e.Sections);
        modelBuilder.Entity<FormSection>()
            .ToTable("form_sections");
        modelBuilder.Entity<FormQuestion>(e =>
        {
            e.ToTable("form_questions");
            e.HasOne<FormQuestion>(fq => fq.NextQuestion);
            e.HasOne<FormQuestion>(fq => fq.NextQuestionAlternative);
            e.Property(p => p.Options).IsRequired()
                .HasJsonColumn(new FormQuestionOptions(),
                    PropertyBuilderExtensions.RecordComparer<FormQuestionOptions>());
            e.Property(p => p.Options)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions.SerializerOptions),
                v => JsonSerializer.Deserialize<FormQuestionOptions>(v, JsonOptions.SerializerOptions) ?? new FormQuestionOptions())
            .HasColumnType("jsonb");
        });


        modelBuilder.Entity<FormAnswerSet>(e =>
        {
            e.ToTable("form_answer_sets");
        });
        modelBuilder.Entity<FormAnswer>(e =>
        {
            e.ToTable("form_answers");
        });
    }

    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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

    public static PropertyBuilder<TProperty> HasJsonColumn<TProperty>(
        this PropertyBuilder<TProperty> propertyBuilder,
        TProperty defaultValue,
        ValueComparer<TProperty> valueComparer
    ) => propertyBuilder
        .HasColumnType("jsonb")
        .HasConversion(
            v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
            v => JsonSerializer.Deserialize<TProperty>(v, JsonSerializerOptions.Default) ?? defaultValue,
            valueComparer
        );
}