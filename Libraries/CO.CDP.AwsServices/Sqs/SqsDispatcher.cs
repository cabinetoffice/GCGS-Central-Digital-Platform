using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ.Pull;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices.Sqs;

public class SqsDispatcher(
    IAmazonSQS sqsClient,
    SqsDispatcherConfiguration configuration,
    Deserializer deserializer,
    TypeMatcher typeMatcher,
    ILogger<SqsDispatcher> logger
) : PullDispatcher<Message>(deserializer, typeMatcher, logger)
{
    private const string TypeAttribute = "Type";

    public SqsDispatcher(
        IAmazonSQS sqsClient,
        IOptions<AwsConfiguration> configuration,
        Deserializer deserializer,
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

    protected override async Task<List<Message>> ReceiveMessages(CancellationToken cancellationToken)
    {
        var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = configuration.QueueUrl,
            MaxNumberOfMessages = configuration.MaxNumberOfMessages,
            MessageAttributeNames = [TypeAttribute],
            WaitTimeSeconds = configuration.WaitTimeSeconds,
        }, cancellationToken);

        logger.LogInformation("Received {COUNT} message(s)", response.Messages.Count);

        return response.Messages;
    }

    protected override async Task DeleteMessage(Message message)
    {
        logger.LogDebug("Deleting the message with MessageId={MessageId}", message.MessageId);

        await sqsClient.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = configuration.QueueUrl,
            ReceiptHandle = message.ReceiptHandle
        });
    }

    protected override string MessageId(Message message) => message.MessageId;
    protected override string MessageBody(Message message) => message.Body;

    protected override string MessageType(Message message) =>
        message.MessageAttributes.GetValueOrDefault(TypeAttribute)?.StringValue ?? "";
}