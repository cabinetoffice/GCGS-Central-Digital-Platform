using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ;

namespace CO.CDP.AwsServices.Sqs;

public delegate object Deserializer(string type, string body);

public delegate bool TypeMatcher(Type type, string typeName);

public record SqsDispatcherConfiguration
{
    public required string QueueName { get; init; }
    public required int MaxNumberOfMessages { get; init; } = 1;
    public required int WaitTimeSeconds { get; init; } = 30;
}

public class SqsDispatcher(
    IAmazonSQS sqsClient,
    SqsDispatcherConfiguration configuration,
    Deserializer deserializer,
    TypeMatcher typeMatcher)
    : IDispatcher
{
    public SqsDispatcher(
        IAmazonSQS sqsClient, SqsDispatcherConfiguration configuration, Deserializer deserializer)
        : this(sqsClient, configuration, deserializer, (type, typeName) => type.Name == typeName)
    {
    }

    private const string TypeAttribute = "Type";
    private readonly SqsSubscribers _subscribers = new();

    public void Subscribe<TM>(Func<TM, Task> subscriber) where TM : class
    {
        _subscribers.Subscribe(subscriber);
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            var queueUrl = await GetQueueUrl(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var message in await ReceiveMessagesAsync(queueUrl, cancellationToken))
                {
                    await HandleMessage(message);
                    await DeleteMessage(queueUrl, message);
                }
            }
        }, cancellationToken);
    }

    private async Task<string> GetQueueUrl(CancellationToken cancellationToken)
    {
        var queue = await sqsClient.GetQueueUrlAsync(configuration.QueueName, cancellationToken);
        return queue.QueueUrl;
    }

    private async Task<List<Message>> ReceiveMessagesAsync(string queueUrl, CancellationToken cancellationToken)
    {
        var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = configuration.MaxNumberOfMessages,
            MessageAttributeNames = [TypeAttribute],
            WaitTimeSeconds = configuration.WaitTimeSeconds,
        }, cancellationToken);
        return response.Messages ?? [];
    }

    private async Task HandleMessage(Message message)
    {
        var type = message.MessageAttributes.GetValueOrDefault(TypeAttribute)?.StringValue ?? "";
        var subscribers = _subscribers.AllMatching((t) => typeMatcher(t, type)).ToList();
        if (subscribers.Any())
        {
            var deserialized = deserializer(type, message.Body);
            foreach (var subscriber in subscribers)
            {
                await subscriber(deserialized);
            }
        }
    }

    private async Task DeleteMessage(string queueUrl, Message message)
    {
        await sqsClient.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = queueUrl,
            ReceiptHandle = message.ReceiptHandle
        });
    }

    public void Dispose()
    {
        sqsClient.Dispose();
    }
}