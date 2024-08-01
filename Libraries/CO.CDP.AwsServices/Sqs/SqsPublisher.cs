using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices.Sqs;

public delegate string Serializer(object message);

public delegate string TypeMapper(Type type);

public class SqsPublisher(
    IAmazonSQS sqsClient,
    SqsPublisherConfiguration configuration,
    Serializer serializer,
    TypeMapper typeMapper,
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
        o => JsonSerializer.Serialize(o),
        type => type.Name,
        logger)
    {
    }

    private static SqsPublisherConfiguration SqsPublisherConfiguration(IOptions<AwsConfiguration> configuration)
    {
        if (configuration.Value.SqsPublisher == null)
        {
            throw new ArgumentNullException(nameof(configuration), "AqsPublisher configuration is missing.");
        }

        return configuration.Value.SqsPublisher;
    }

    public async Task Publish<TM>(TM message) where TM : notnull
    {
        var messageType = typeMapper(typeof(TM));
        var serialized = serializer(message);

        logger.LogDebug("Publishing the `{TYPE}` message: `{MESSAGE}`", messageType, serialized);

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = configuration.QueueUrl,
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
}