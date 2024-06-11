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

            entity.OwnsMany(e => e.Identifiers, a =>
            {
                a.HasKey(e => e.Id);
                a.Property(ai => ai.Primary);
                a.Property(ai => ai.IdentifierId);
                a.Property(ai => ai.Scheme);
                a.Property(ai => ai.LegalName);
                a.Property(ai => ai.Uri);
                a.Property(ai => ai.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                a.Property(ai => ai.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            entity.OwnsOne(e => e.SupplierInfo, a =>
            {
                a.Property(z => z.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                a.Property(z => z.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                a.ToTable("SupplierInformation");

                a.OwnsMany(x => x.Qualifications, y =>
                {
                    y.HasKey(z => z.Id);
                    y.Property(z => z.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                    y.Property(z => z.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                    y.ToTable("Qualification");
                });

                a.OwnsMany(x => x.TradeAssurances, y =>
                {
                    y.HasKey(z => z.Id);
                    y.Property(z => z.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                    y.Property(z => z.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                    y.ToTable("TradeAssurance");
                });
                a.OwnsOne(x => x.LegalForm, y =>
                {
                    y.Property(z => z.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                    y.Property(z => z.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                    y.ToTable("LegalForm");
                });
            });

            entity.OwnsOne(e => e.BuyerInfo, a =>
            {
                a.Property(z => z.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                a.Property(z => z.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                a.ToTable("BuyerInformation");
            });

            entity
                .HasMany(p => p.Persons)
                .WithMany(t => t.Organisations)
                .UsingEntity<OrganisationPerson>(j =>
                {
                    j.Property(op => op.CreatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                    j.Property(op => op.UpdatedOn).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                });

            entity.OwnsMany(e => e.Addresses, a => { a.HasKey(e => e.Id); });
        });

        modelBuilder.Entity<Person>()
            .HasMany(p => p.Tenants)
            .WithMany(t => t.Persons)
            .UsingEntity<TenantPerson>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityDate).IsAssignableFrom(entityType.ClrType) && !entityType.IsOwned())
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