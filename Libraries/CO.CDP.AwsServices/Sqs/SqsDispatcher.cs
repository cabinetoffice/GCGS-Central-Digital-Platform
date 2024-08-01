using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices.Sqs;

public delegate object Deserializer(string type, string body);

public delegate bool TypeMatcher(Type type, string typeName);

public class SqsDispatcher(
    IAmazonSQS sqsClient,
    SqsDispatcherConfiguration configuration,
    Deserializer deserializer,
    TypeMatcher typeMatcher,
    ILogger<SqsDispatcher> logger
) : IDispatcher
{
    public SqsDispatcher(
        IAmazonSQS sqsClient,
        IOptions<AwsConfiguration>
            configuration, Deserializer deserializer,
        ILogger<SqsDispatcher> logger
    ) : this(
        sqsClient,
        SqsDispatcherConfiguration(configuration),
        deserializer,
        (type, typeName) => type.Name == typeName,
        logger)
    {
    }

    private static SqsDispatcherConfiguration SqsDispatcherConfiguration(IOptions<AwsConfiguration> configuration)
    {
        if (configuration.Value.SqsDispatcher == null)
        {
            throw new ArgumentNullException(nameof(configuration), "SqsDispatcher configuration is missing.");
        }

        return configuration.Value.SqsDispatcher;
    }

    private const string TypeAttribute = "Type";
    private readonly SqsSubscribers _subscribers = new();

    public void Subscribe<TM>(ISubscriber<TM> subscriber) where TM : class
    {
        _subscribers.Subscribe(subscriber);
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            logger.LogInformation("Started the SQS message dispatcher");

            while (!cancellationToken.IsCancellationRequested)
            {
                await HandleMessages(cancellationToken);
            }
        }, cancellationToken);
    }

    private async Task HandleMessages(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var message in await ReceiveMessages(configuration.QueueUrl, cancellationToken))
            {
                await HandleMessage(message);
            }
        }
        catch (Exception cause)
        {
            logger.LogError(cause, "Failed to handle messages");
        }
    }

    private async Task HandleMessage(Message message)
    {
        try
        {
            await InvokeSubscribers(message);
            await DeleteMessage(message);
        }
        catch (Exception cause)
        {
            logger.LogError(cause, "Failed to handle the message with MessageId={MessageId}", message.MessageId);
        }
    }

    private async Task InvokeSubscribers(Message message)
    {
        logger.LogDebug("Handling the message with MessageId={MessageId}", message.MessageId);

        var type = message.MessageAttributes.GetValueOrDefault(TypeAttribute)?.StringValue ?? "";
        var matchingSubscribers = _subscribers.AllMatching((t) => typeMatcher(t, type)).ToList();

        logger.LogDebug("Found {CNT} subscribers to handle the `{TYPE}` message", matchingSubscribers.Count, type);

        if (matchingSubscribers.Any())
        {
            var deserialized = deserializer(type, message.Body);

            logger.LogDebug("Handling the `{TYPE}` message: `{MESSAGE}`", type, message.Body);

            foreach (var subscriber in matchingSubscribers)
            {
                await subscriber.Handle(deserialized);
            }
        }
    }

    private async Task<List<Message>> ReceiveMessages(string queueUrl, CancellationToken cancellationToken)
    {
        var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = configuration.MaxNumberOfMessages,
            MessageAttributeNames = [TypeAttribute],
            WaitTimeSeconds = configuration.WaitTimeSeconds,
        }, cancellationToken);

        logger.LogInformation("Received {COUNT} message(s)", response.Messages.Count);

        return response.Messages;
    }

    private async Task DeleteMessage(Message message)
    {
        logger.LogDebug("Deleting the message with MessageId={MessageId}", message.MessageId);

        await sqsClient.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = configuration.QueueUrl,
            ReceiptHandle = message.ReceiptHandle
        });
    }

    public void Dispose()
    {
    }
}