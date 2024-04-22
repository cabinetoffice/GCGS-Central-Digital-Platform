using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Organisation.Persistence;
public class OrganisationContext(string connectionString) : DbContext
{
    public DbSet<Organisation> Organisations { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }

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
            });

            entity.OwnsMany(e => e.AdditionalIdentifiers, a =>
            {
                a.WithOwner();
                a.Property(ai => ai.Id);
                a.Property(ai => ai.Scheme);
                a.Property(ai => ai.LegalName);
                a.Property(ai => ai.Uri);
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}