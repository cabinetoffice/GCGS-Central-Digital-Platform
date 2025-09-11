using Microsoft.EntityFrameworkCore;

namespace CO.CDP.RegisterOfCommercialTools.Persistence;

public class RegisterOfCommercialToolsContext(DbContextOptions<RegisterOfCommercialToolsContext> options) : DbContext(options)
{
    public DbSet<CpvCode> CpvCodes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureCpvCodes();
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(optionsBuilder);
    }
}