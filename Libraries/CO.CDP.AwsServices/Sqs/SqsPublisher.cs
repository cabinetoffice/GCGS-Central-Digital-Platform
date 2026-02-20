using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ;
using CO.CDP.MQ.Outbox;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices.Sqs;

public class SqsPublisher(
    IAmazonSQS sqsClient,
    SqsPublisherConfiguration configuration,
    Func<object, string> serializer,
    Func<object, string> typeMapper,
    ILogger<SqsPublisher> logger
) : IPublisher
{
    private const string TypeAttribute = "Type";

    public SqsPublisher(IAmazonSQS sqsClient, IOptions<AwsConfiguration> configuration, ILogger<SqsPublisher> logger)
        : this(sqsClient, SqsPublisherConfiguration(configuration), logger)
    {
    }

    public SqsPublisher(IAmazonSQS sqsClient, SqsPublisherConfiguration configuration, ILogger<SqsPublisher> logger) : this(
        sqsClient,
        configuration,
        OutboxMessageSerializerFactory.Create(o => JsonSerializer.Serialize(o)),
        OutboxMessageTypeMapperFactory.Create(o => o.GetType().Name),
        logger)
    {
    }

    private static SqsPublisherConfiguration SqsPublisherConfiguration(IOptions<AwsConfiguration> configuration)
    {
        if (configuration.Value.SqsPublisher == null)
        {
            throw new ArgumentNullException(nameof(configuration), "SqsPublisher configuration is missing.");
        }

        return configuration.Value.SqsPublisher;
    }

    public async Task Publish<TM>(TM message) where TM : notnull
    {
        var messageType = typeMapper(message);
        var serialized = serializer(message);

        logger.LogDebug("Publishing the `{TYPE}` message: `{MESSAGE}`", messageType, serialized);

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = QueueUrlFor(message),
            MessageGroupId = MessageGroupIdFor(message),
            MessageBody = serialized,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    TypeAttribute,
                    new MessageAttributeValue { StringValue = messageType, DataType = "String" }
                }
            }
        });
    }

    public void Dispose()
    {
    }

    private string QueueUrlFor<TM>(TM _) where TM : notnull
    {
        return configuration.QueueUrl;
    }

    private string? MessageGroupIdFor<TM>(TM _) where TM : notnull
    {
        return configuration.MessageGroupId;
    }
}
