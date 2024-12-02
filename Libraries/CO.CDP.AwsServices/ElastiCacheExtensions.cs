using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices;

public static class ElastiCacheExtensions
{
    public static IServiceCollection AddElastiCacheService(this IServiceCollection services)
    {
        return services
            .AddElastiCacheOptions();
    }

    private static IServiceCollection AddElastiCacheOptions(this IServiceCollection services)
    {
        var awsConfiguration = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AwsConfiguration>>().Value;
        ;

        if (awsConfiguration.ElastiCache is null)
        {
            return services;
        }

        return services.AddStackExchangeRedisCache(options =>
        {
            var redisPort = awsConfiguration.ElastiCache.Port;
            var redisHost = awsConfiguration.ElastiCache.Hostname;

            options.Configuration = $"{redisHost}:{redisPort}";
            options.InstanceName = "Sessions";
        });
    }
}