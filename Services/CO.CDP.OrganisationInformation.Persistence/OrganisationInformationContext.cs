using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationInformationContext(DbContextOptions<OrganisationInformationContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Organisation> Organisations { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.OwnsOne(e => e.Identifier, b =>
            {
                b.Property(p => p.Id);
                b.Property(p => p.Scheme);
                b.Property(p => p.LegalName);
                b.Property(p => p.Uri);
                b.Property(p => p.Number);
            });

            entity.OwnsMany(e => e.AdditionalIdentifiers, a =>
            {
                a.WithOwner();
                a.Property(ai => ai.Id);
                a.Property(ai => ai.Scheme);
                a.Property(ai => ai.LegalName);
                a.Property(ai => ai.Uri);
                a.Property(ai => ai.Number);
            });

            modelBuilder.Entity<Person>()
                .HasMany(p => p.Tenants)
                .WithMany(t => t.Persons)
                .UsingEntity<TenantPerson>();
        });
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id);
            modelBuilder.Entity<Organisation>()
                .HasMany(p => p.Persons)
                .WithMany(t => t.Organisations)
                .UsingEntity<OrganisationPerson>();
        });

       foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityDate).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>("CreatedOn").IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>("UpdatedOn").IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }

        base.OnModelCreating(modelBuilder);
    }
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries<IEntityDate>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Entity.CreatedOn = DateTimeOffset.UtcNow;
            }

            entityEntry.Entity.UpdatedOn = DateTimeOffset.UtcNow;
        }
    }

}