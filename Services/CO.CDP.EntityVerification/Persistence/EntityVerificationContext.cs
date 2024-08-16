using System.Text.Json;
using CO.CDP.EntityFrameworkCore.Timestamps;
using CO.CDP.EntityVerification.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CO.CDP.EntityVerification.Persistence;


public class EntityVerificationContext : DbContext
{
    public DbSet<Ppon> Ppons { get; set; } = null!;
    public DbSet<Identifier> Identifiers { get; set; } = null!;

    public EntityVerificationContext()
    {
    }

    public EntityVerificationContext(DbContextOptions<EntityVerificationContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("entity_verification");

        modelBuilder.Entity<Ppon>(ppon =>
        {
            ppon.HasIndex(p => p.IdentifierId).IsUnique();
            ppon.HasIndex(p => p.OrganisationId);
            ppon.HasIndex(p => p.Name);
            ppon.Property(p => p.CreatedOn).HasTimestampDefault();
            ppon.Property(p => p.UpdatedOn).HasTimestampDefault();
        });

        modelBuilder.Entity<Identifier>(identifier =>
        {
            identifier.HasKey(p => p.Id);
            identifier.HasIndex(p => p.IdentifierId);
            identifier.HasIndex(p => p.Scheme);
            identifier.Property(p => p.CreatedOn).HasTimestampDefault();
            identifier.Property(p => p.UpdatedOn).HasTimestampDefault();
            identifier.HasOne<Ppon>()
                .WithMany(p => p.Identifiers)
                .HasForeignKey("PponId")
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
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