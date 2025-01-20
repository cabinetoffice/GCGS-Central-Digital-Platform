using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace CO.CDP.AwsServices;

public static class ElastiCacheExtensions
{
    public static IServiceCollection AddSharedSessions(this IServiceCollection services,
        IConfiguration configuration)
    {
        var logger = services.BuildServiceProvider()
            .GetRequiredService<ILogger>();

        if (configuration.GetValue<bool>("Features:SharedSessions"))
        {
            try
            {
                services.AddElastiCacheService(logger);
                logger.Information("SharedSession is enabled.");

                return services;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SharedSession failed to start.");

                // @TODO: remove this line once trigger for sticky sessions is added - see DP-1084 for details
                throw new Exception("SharedSession failed to start.", ex);
            }
        }

        services.AddDistributedMemoryCache();
        logger.Warning("SharedSession is disabled, local Sessions in use.");

        return services;
    }

    private static IServiceCollection AddElastiCacheService(this IServiceCollection services, ILogger logger)
    {
        return services.AddElastiCacheOptions(logger);
    }

    private static IServiceCollection AddElastiCacheOptions(this IServiceCollection services, ILogger logger)
    {
        var awsConfiguration = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AwsConfiguration>>().Value;

        if (awsConfiguration.ElastiCache is null)
        {
            throw new Exception("ElastiCache is not configured.");
        }

        return services.AddStackExchangeRedisCache(options =>
        {
            var redisPort = awsConfiguration.ElastiCache.Port;
            var redisHost = awsConfiguration.ElastiCache.Hostname;
            logger.Verbose($"ElastiCache service in use: {redisHost}:{redisPort}");
            options.Configuration = $"{redisHost}:{redisPort}";
            options.InstanceName = "Sessions";
        });
    }
}