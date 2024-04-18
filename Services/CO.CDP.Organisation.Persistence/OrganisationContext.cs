using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Organisation.Persistence;
public class OrganisationContext(string connectionString) : DbContext
{
    public DbSet<Organisation> Organisations { get; set; } = null!;
    //public DbSet<OrganisationIdentifier> OrganisationIdentifier { get; set; } = null!;
    //public DbSet<OrganisationAddress> OrganisationAddress { get; set; } = null!;
    //public DbSet<OrganisationContactPoint> OrganisationContactPoint { get; set; } = null!;

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
                b.Property(p => p.Id).HasColumnName("IdentifierId");
                b.Property(p => p.Scheme).HasColumnName("Scheme");
                b.Property(p => p.LegalName).HasColumnName("LegalName");
                b.Property(p => p.Uri).HasColumnName("Uri");
            });

            entity.OwnsMany(e => e.AdditionalIdentifiers, a =>
            {
                a.WithOwner();
                a.Property(ai => ai.Id).HasColumnName("IdentifierId");
                a.Property(ai => ai.Scheme).HasColumnName("Scheme");
                a.Property(ai => ai.LegalName).HasColumnName("LegalName");
                a.Property(ai => ai.Uri).HasColumnName("Uri");
                a.ToTable("AdditionalIdentifiers");
            });

            entity.OwnsOne(e => e.Address);
            entity.OwnsOne(e => e.ContactPoint);
        });

        base.OnModelCreating(modelBuilder);
    }
}