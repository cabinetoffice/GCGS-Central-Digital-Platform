using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CO.CDP.ApplicationRegistry.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationRegistryContext>
{
    public ApplicationRegistryContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationRegistryContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=organisation_information;Username=postgres;Password=postgres");
        return new ApplicationRegistryContext(optionsBuilder.Options);
    }
}
