using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using CO.CDP.AwsServices.S3;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    public static IServiceCollection AddSqsPublisher(this IServiceCollection services)
    {
        return services.AddScoped<IPublisher, SqsPublisher>();
    }

    public static IServiceCollection AddSqsDispatcher(this IServiceCollection services,
        Deserializer deserializer,
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
        return services;
    }

    public static void Subscribe<TEvent>(this IDispatcher dispatcher, IServiceProvider services) where TEvent : class
    {
        dispatcher.Subscribe(services.GetRequiredService<ISubscriber<TEvent>>());
    }
}