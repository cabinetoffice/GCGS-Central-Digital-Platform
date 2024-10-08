using Amazon.CloudWatchLogs;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;

namespace CO.CDP.AwsServices;

public static class CloudWatchExtensions
{
    public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.Configure<LoggingConfiguration>(configuration.GetSection("Logging"));
    }

    public static IServiceCollection AddAmazonCloudWatchLogsService(this IServiceCollection services)
    {
        return services
            .AddCloudWatchSinkOptions()
            .AddCloudWatchLogs();
    }

    private static IServiceCollection AddCloudWatchSinkOptions(this IServiceCollection services)
    {
        services.Add(new ServiceDescriptor(typeof(ICloudWatchSinkOptions), serviceProvider =>
        {
            var awsConfiguration = serviceProvider.GetRequiredService<IOptions<AwsConfiguration>>().Value;
            // options for the sink defaults in https://github.com/Cimpress-MCP/serilog-sinks-awscloudwatch/blob/master/src/Serilog.Sinks.AwsCloudWatch/CloudWatchSinkOptions.cs
            return awsConfiguration.CloudWatch is not null ? new CloudWatchSinkOptions
            {
                LogGroupName = awsConfiguration.CloudWatch.LogGroup,
                TextFormatter = new RenderedCompactJsonFormatter(),
                LogStreamNameProvider = new ConfigurableLogStreamNameProvider(awsConfiguration.CloudWatch.LogStream),
                MinimumLogEventLevel = LogEventLevel.Verbose
            } : throw new ConfigurationException("Missing CloudWatch configuration.");
        }, ServiceLifetime.Singleton));
        return services;
    }

    private static IServiceCollection AddCloudWatchLogs(this IServiceCollection services)
    {
        return services.AddSingleton<IAmazonCloudWatchLogs>(serviceProvider =>
        {
            var awsConfiguration = serviceProvider.GetRequiredService<IOptions<AwsConfiguration>>().Value;

            return awsConfiguration.Credentials is not null && awsConfiguration.ServiceURL is not null
                ? new AmazonCloudWatchLogsClient(
                    new BasicAWSCredentials(awsConfiguration.Credentials.AccessKeyId,
                        awsConfiguration.Credentials.SecretAccessKey),
                    new AmazonCloudWatchLogsConfig
                    {
                        ServiceURL = awsConfiguration.ServiceURL
                    })
                : new AmazonCloudWatchLogsClient();
        });
    }

    public static IServiceCollection AddCloudWatchSerilog(
        this IServiceCollection services, IConfiguration configuration)
    {
        return AddCloudWatchSerilog(services, configuration, (_, _) => { });
    }

    private static IServiceCollection AddCloudWatchSerilog(
        this IServiceCollection services, IConfiguration configuration,
        Action<IServiceProvider, LoggerConfiguration> configureLogger)
    {
        return services.AddSerilog((serviceProvider, lc) =>
        {
            var loggingConfiguration = serviceProvider.GetRequiredService<IOptions<LoggingConfiguration>>().Value;
            lc.WriteTo.AmazonCloudWatch(serviceProvider)
                .MinimumLevel.Is(loggingConfiguration.MinimumLevel.Default)
                .OverrideLogLevels(loggingConfiguration.MinimumLevel.Override)
                .EnableConsole(loggingConfiguration.Console)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Version", configuration.GetValue<string>("Version"));
            configureLogger(serviceProvider, lc);
        });
    }

    private static LoggerConfiguration OverrideLogLevels(this LoggerConfiguration configuration,
        Dictionary<string, LogEventLevel> logLevels)
    {
        return logLevels.Aggregate(configuration,
            (c, o) => c.MinimumLevel.Override(o.Key, o.Value));
    }

    private static LoggerConfiguration EnableConsole(this LoggerConfiguration configuration, bool enable)
    {
        return enable ? configuration.WriteTo.Console() : configuration;
    }

    private static LoggerConfiguration AmazonCloudWatch(this LoggerSinkConfiguration loggerConfiguration,
        IServiceProvider serviceProvider)
    {
        return loggerConfiguration.AmazonCloudWatch(
            serviceProvider.GetRequiredService<ICloudWatchSinkOptions>(),
            serviceProvider.GetRequiredService<IAmazonCloudWatchLogs>()
        );
    }
}