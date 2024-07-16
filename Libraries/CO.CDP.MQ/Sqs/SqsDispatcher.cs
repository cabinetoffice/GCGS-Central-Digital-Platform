using Amazon.SQS;
using Amazon.SQS.Model;

namespace CO.CDP.MQ.Sqs;

public delegate object Deserializer(string type, string body);

public delegate bool TypeMatcher(Type type, string typeName);

public class SqsDispatcher(
    AmazonSQSClient sqsClient,
    string queueUrl,
    Deserializer deserializer,
    TypeMatcher typeMatcher)
    : IDispatcher
{
    public SqsDispatcher(AmazonSQSClient sqsClient, string queueUrl, Deserializer deserializer) : this(
        sqsClient, queueUrl, deserializer, (type, typeName) => type.Name == typeName)
    {
    }

    private const string TypeAttribute = "Type";
    private readonly Dictionary<Type, List<Func<object, Task>>> _subscribers = [];

    public void Subscribe<TM>(Func<TM, Task> subscriber) where TM : class
    {
        var subscribers = _subscribers.GetValueOrDefault(typeof(TM), []);
        subscribers.Add(o => subscriber((TM)o));
        _subscribers[typeof(TM)] = subscribers;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var message in await ReceiveMessagesAsync(cancellationToken))
                {
                    await HandleMessage(message);
                    await DeleteMessage(message);
                }
            }
        }, cancellationToken);
    }

    private async Task<List<Message>> ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 1,
            MessageAttributeNames = [TypeAttribute]
        }, cancellationToken);
        return response.Messages ?? [];
    }

    private async Task HandleMessage(Message message)
    {
        var type = message.MessageAttributes.GetValueOrDefault(TypeAttribute)?.StringValue ?? "";
        foreach (var subscribers in _subscribers)
        {
            if (typeMatcher(subscribers.Key, type))
            {
                var deserialized = deserializer(type, message.Body);
                foreach (var subscriber in subscribers.Value)
                {
                    await subscriber(deserialized);
                }
            }
        }
    }

    private async Task DeleteMessage(Message message)
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