using Amazon.Runtime;
using Amazon.SQS;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Persistence;
using CO.CDP.EntityVerification.Services;
using CO.CDP.MQ;
using CO.CDP.MQ.Sqs;

namespace CO.CDP.EntityVerification.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly Dictionary<Type, (int, string)> ExceptionMap = new()
    {
        { typeof(BadHttpRequestException), (StatusCodes.Status422UnprocessableEntity, "UNPROCESSABLE_ENTITY") },
    };

    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services,
        ConfigurationManager config)
    {
        services.AddScoped<AmazonSQSClient>(s =>
        {
            var sqsConfig = new AmazonSQSConfig
            {
                ServiceURL = config.GetValue("Sqs:ServiceURL", ""),
                UseHttp = false,
                AuthenticationRegion = config.GetValue("Sqs:AuthenticationRegion", "")
            };
            var credentials = new BasicAWSCredentials(
                config.GetValue("Sqs:AccessKey", ""),
                config.GetValue("Sqs:SecretKey", ""));
            return new AmazonSQSClient(credentials, sqsConfig);
        });
        services.AddScoped<IDispatcher, SqsDispatcher>(s =>
        {
            var dispatcher = new SqsDispatcher(
                s.GetRequiredService<AmazonSQSClient>(),
                new SqsDispatcherConfiguration
                {
                    QueueName = config.GetValue("InboundQueue:Name", "") ?? "",
                    WaitTimeSeconds = config.GetValue("InboundQueue:WaitTimeSeconds", 20),
                    MaxNumberOfMessages = config.GetValue("InboundQueue:MaxNumberOfMessages", 10)
                },
                EventDeserializer.Deserializer);
            dispatcher.Subscribe<OrganisationRegistered>(message =>
            {
                s.GetRequiredService<OrganisationRegisteredEventHandler>().Action(message);
                return Task.CompletedTask;
            });
            return dispatcher;
        });
        services.AddScoped<IPponService, PponService>();
        services.AddScoped<OrganisationRegisteredEventHandler>();
        services.AddScoped<IPponRepository, DatabasePponRepository>();

        services.AddHostedService<QueueBackgroundService>();

        return services;
    }

    public static IServiceCollection AddEntityVerificationProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                if (ctx.Exception != null)
                {
                    var (statusCode, errorCode) = MapException(ctx.Exception);
                    ctx.ProblemDetails.Status = statusCode;
                    ctx.HttpContext.Response.StatusCode = statusCode;
                    ctx.ProblemDetails.Extensions.Add("code", errorCode);
                }
            };
        });

        return services;
    }

    public static (int status, string error) MapException(Exception? exception)
    {
        if (ExceptionMap.TryGetValue(exception?.GetType() ?? typeof(Exception), out (int, string) code))
        {
            return code;
        }

        return (StatusCodes.Status500InternalServerError, "GENERIC_ERROR");
    }

    public static Dictionary<string, List<string>> ErrorCodes() =>
        ExceptionMap.Values
            .GroupBy(s => s.Item1)
            .ToDictionary(k => k.Key.ToString(), v => v.Select(i => i.Item2).Distinct().ToList());
}