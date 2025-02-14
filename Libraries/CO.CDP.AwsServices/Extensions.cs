using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using CO.CDP.AwsServices.S3;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.MQ;
using CO.CDP.MQ.Hosting;
using CO.CDP.MQ.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CO.CDP.AwsServices;

public static class Extensions
{
    public static IServiceCollection AddAwsConfiguration(
        this IServiceCollection services, IConfiguration configuration)
    {
        var awsSection = configuration.GetSection("Aws");

        var awsConfig = awsSection.Get<AwsConfiguration>()
                        ?? throw new Exception("Aws environment configuration missing.");

        AddAwsOptions(services, awsConfig);

        services.Configure<AwsConfiguration>(awsSection);
        return services;
    }

    /**
     * Only adds AWS Options if Credentials or ServiceURL were defined.
     * This is mostly to support local development environment based on localstack.
     * In production, the default process for finding credentials kicks in,
     * and finds them in IAM Roles for tasks.
     * See https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/creds-assign.html
     */
    private static void AddAwsOptions(IServiceCollection services, AwsConfiguration awsConfig)
    {
        var awsOptions = new AWSOptions();

        if (awsConfig.Credentials is not null)
        {
            awsOptions.Credentials =
                new BasicAWSCredentials(awsConfig.Credentials.AccessKeyId, awsConfig.Credentials.SecretAccessKey);
        }

        if (!string.IsNullOrWhiteSpace(awsConfig.ServiceURL))
        {
            awsOptions.DefaultClientConfig.ServiceURL = awsConfig.ServiceURL;
        }

        if (awsOptions.Credentials is not null || awsOptions.DefaultClientConfig.ServiceURL is not null)
        {
            services.AddDefaultAWSOptions(awsOptions);
        }
    }

    public static IServiceCollection AddAwsS3Service(this IServiceCollection services)
    {
        return services
            .AddAWSService<IAmazonS3>()
            .AddSingleton<ITransferUtility, TransferUtilityWrapper>()
            .AddSingleton<IFileHostManager, AwsFileManager>();
    }

    public static IServiceCollection AddAwsSqsService(this IServiceCollection services)
    {
        return services
            .AddAWSService<IAmazonSQS>();
    }

    public static IServiceCollection AddOutboxSqsPublisher<TDbContext>(this IServiceCollection services, IConfiguration configuration)
        where TDbContext : DbContext, IOutboxMessageDbContext
    {
        var context = configuration.GetValue<string>("DbContext");
        if (context == "OrganisationInformationContext")
        {
            services.AddDbContext<TDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
            services.AddScoped<IOutboxMessageRepository, DatabaseOutboxMessageRepository<TDbContext>>();
        }
        else
        {
            services.AddDbContext<TDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>()));
            services.AddScoped<IOutboxMessageRepository, DatabaseOutboxMessageRepository<TDbContext>>();
        }

        services.AddSingleton<OutboxProcessorBackgroundService.OutboxProcessorConfiguration>(s =>
                 s.GetRequiredService<IOptions<AwsConfiguration>>().Value.SqsPublisher?.Outbox ??
                 new OutboxProcessorBackgroundService.OutboxProcessorConfiguration()
             );
        services.AddKeyedScoped<IPublisher, SqsPublisher>("SqsPublisher");
        services.AddScoped<IOutboxProcessor>(s =>
        {
            return new OutboxProcessor(
                s.GetRequiredKeyedService<IPublisher>("SqsPublisher"),
                s.GetRequiredService<IOutboxMessageRepository>(),
                s.GetRequiredService<ILogger<OutboxProcessor>>()
            );
        });

        services.AddScoped<IOutboxProcessorListener>(s => new OutboxProcessorListener(
            s.GetRequiredService<NpgsqlDataSource>(),
            s.GetRequiredService<IOutboxProcessor>(),
            s.GetRequiredService<ILogger<OutboxProcessorListener>>(),
            channel: configuration["channel"]!
        ));
        services.AddHostedService<OutboxProcessorListenerBackgroundService>();

        return services;
    }

    public static IServiceCollection AddOutboxSqsPublisher<TDbContext>(
        this IServiceCollection services,
        ConfigurationManager configuration,
        bool enableBackgroundServices,
        string notificationChannel
    ) where TDbContext : DbContext, IOutboxMessageDbContext
    {
        var awsSection = configuration.GetSection("Aws");
        var awsConfig = awsSection.Get<AwsConfiguration>()
                        ?? throw new Exception("Aws environment configuration missing.");

        if ((string.IsNullOrEmpty(awsConfig?.SqsPublisher?.QueueUrl)) || (string.IsNullOrEmpty(awsConfig?.SqsPublisher?.MessageGroupId)))
        {
            throw new ArgumentNullException(nameof(awsConfig), "SqsPublisher QueueUrl / MessageGroupId is missing.");
        }

        services.AddScoped<IOutboxMessageRepository, DatabaseOutboxMessageRepository<TDbContext>>();
        services.AddKeyedScoped<IPublisher, SqsPublisher>("SqsPublisher");
        services.AddScoped<IPublisher, OutboxMessagePublisher>();

        services.AddSingleton<OutboxProcessorBackgroundService.OutboxProcessorConfiguration>(s =>
            s.GetRequiredService<IOptions<AwsConfiguration>>().Value.SqsPublisher?.Outbox ??
            new OutboxProcessorBackgroundService.OutboxProcessorConfiguration()
        );

        services.AddSingleton<OutboxMessagePublisher.OutboxMessagePublisherConfiguration>(s =>
        {
            var awsConfig = s.GetRequiredService<IOptions<AwsConfiguration>>().Value.SqsPublisher;

            return new OutboxMessagePublisher.OutboxMessagePublisherConfiguration
            {
                QueueUrl = awsConfig?.QueueUrl ?? "",
                MessageGroupId = awsConfig?.MessageGroupId ?? ""
            };
        });

        if (!configuration.GetValue("Features:OutboxProcessorBackgroundService", false))
        {
            services.AddKeyedScoped<IPublisher, SqsPublisher>("SqsPublisher");
            services.AddScoped<IOutboxProcessor>(s =>
            {
                return new OutboxProcessor(
                    s.GetRequiredKeyedService<IPublisher>("SqsPublisher"),
                    s.GetRequiredService<IOutboxMessageRepository>(),
                    s.GetRequiredService<ILogger<OutboxProcessor>>()
                );
            });

            if (enableBackgroundServices)
            {
                services.AddHostedService<OutboxProcessorBackgroundService>();
            }
        }

        return services;
    }

    public static IServiceCollection AddSqsDispatcher(this IServiceCollection services,
        Deserializer deserializer,
        bool enableBackgroundServices,
        Action<IServiceCollection> addSubscribers,
        Action<IServiceProvider, IDispatcher> registerSubscribers)
    {
        services.AddScoped<Deserializer>(_ => deserializer);

        addSubscribers(services);

        services.AddScoped<IDispatcher, SqsDispatcher>(serviceProvider =>
        {
            var dispatcher = new SqsDispatcher(
                serviceProvider.GetRequiredService<IAmazonSQS>(),
                serviceProvider.GetRequiredService<IOptions<AwsConfiguration>>(),
                serviceProvider.GetRequiredService<Deserializer>(),
                serviceProvider.GetRequiredService<ILogger<SqsDispatcher>>());
            registerSubscribers(serviceProvider, dispatcher);
            return dispatcher;
        });
        if (enableBackgroundServices)
        {
            services.AddHostedService<DispatcherBackgroundService>();
        }
        return services;
    }

    public static void Subscribe<TEvent>(this IDispatcher dispatcher, IServiceProvider services) where TEvent : class
    {
        dispatcher.Subscribe(services.GetRequiredService<ISubscriber<TEvent>>());
    }
}