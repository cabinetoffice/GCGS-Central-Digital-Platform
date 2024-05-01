using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Persistence.OrganisationInformation;

public class TenantContext(string connectionString) : DbContext
{
    public DbSet<Tenant> Tenants { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }
}