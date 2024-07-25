using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using CO.CDP.AwsServices.S3;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices;

public static class Extensions
{
    public static IServiceCollection AddAwsCofiguration(
        this IServiceCollection services, IConfiguration configuration)
    {
        var awsSection = configuration.GetSection("Aws");

        var awsConfig = awsSection.Get<AwsConfiguration>()
            ?? throw new Exception("Aws environment configuration missing.");

        if (!string.IsNullOrWhiteSpace(awsConfig.ServiceURL) && !string.IsNullOrWhiteSpace(awsConfig.Region))
            throw new Exception("Either provide aws service url or aws region endpoint configuration.");

        var awsOptions = new AWSOptions
        {
            Credentials = new BasicAWSCredentials(awsConfig.AccessKeyId, awsConfig.SecretAccessKey)
        };

        if (!string.IsNullOrWhiteSpace(awsConfig.ServiceURL))
            awsOptions.DefaultClientConfig.ServiceURL = awsConfig.ServiceURL;

        if (!string.IsNullOrWhiteSpace(awsConfig.Region))
            awsOptions.Region = RegionEndpoint.GetBySystemName(awsConfig.Region);

        return services
            .Configure<AwsConfiguration>(awsSection)
            .AddDefaultAWSOptions(awsOptions);
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
                serviceProvider.GetRequiredService<Deserializer>());
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