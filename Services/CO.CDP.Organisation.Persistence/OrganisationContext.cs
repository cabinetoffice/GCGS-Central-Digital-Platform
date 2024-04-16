using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Organisation.Persistence;
public class OrganisationContext(string connectionString) : DbContext
{
    public DbSet<Organisation> Organisations { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }
}