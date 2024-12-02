using Microsoft.Extensions.Configuration;
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

    public static IServiceCollection AddSharedSessions(this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Features:SharedSessions"))
        {
            Console.WriteLine("SharedSession is enabled.");
            services
                .AddElastiCacheService();
        }
        else
        {
            Console.WriteLine("SharedSession is disabled.");
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    private static IServiceCollection AddElastiCacheOptions(this IServiceCollection services)
    {
        var awsConfiguration = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AwsConfiguration>>().Value;
        ;

        if (awsConfiguration.ElastiCache is null)
        {
            Console.WriteLine("No elasti cache service configured.");
            return services;
        }

        Console.WriteLine($"Elasti Cache Options: {awsConfiguration.ElastiCache}");
        return services.AddStackExchangeRedisCache(options =>
        {
            var redisPort = awsConfiguration.ElastiCache.Port;
            var redisHost = awsConfiguration.ElastiCache.Hostname;
            Console.WriteLine($"Elasti Cache Options: {redisHost}:{redisPort}");
            options.Configuration = $"{redisHost}:{redisPort}";
            options.InstanceName = "Sessions";
        });
    }
}