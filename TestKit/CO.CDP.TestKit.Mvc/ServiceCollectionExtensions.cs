using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.TestKit.Mvc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureInMemoryDbContext<TC>(this IServiceCollection services)
        where TC : DbContext =>
        services.AddScoped<DbContextOptions<TC>>(_ =>
            new DbContextOptionsBuilder<TC>()
                .UseInMemoryDatabase(databaseName: $"db-{Guid.NewGuid()}")
                .Options
        );
}