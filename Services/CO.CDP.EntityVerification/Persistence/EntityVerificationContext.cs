using CO.CDP.EntityVerification.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;

namespace CO.CDP.EntityVerification.Persistence;

public class EntityVerificationContext : DbContext
{
    public DbSet<Ppon> Ppons { get; set; } = null!;

    public EntityVerificationContext()
    {
    }

    public EntityVerificationContext(DbContextOptions<EntityVerificationContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("entity_verification");

        modelBuilder.Entity<Ppon>()
            .ToTable("ppon");

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.ReplaceService<IHistoryRepository, CamelCaseHistoryContext>();
        base.OnConfiguring(optionsBuilder);
    }

    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
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