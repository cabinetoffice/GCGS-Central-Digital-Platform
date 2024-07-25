using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ;
using Microsoft.Extensions.Options;

namespace CO.CDP.AwsServices.Sqs;

public delegate string Serializer(object message);

public delegate string TypeMapper(Type type);

public class SqsPublisher(
    IAmazonSQS sqsClient,
    SqsPublisherConfiguration configuration,
    Serializer serializer,
    TypeMapper typeMapper) : IPublisher
{
    private const string TypeAttribute = "Type";

    public SqsPublisher(IAmazonSQS sqsClient, IOptions<AwsConfiguration> configuration)
        : this(sqsClient, SqsPublisherConfiguration(configuration))
    {
    }

    public SqsPublisher(IAmazonSQS sqsClient, SqsPublisherConfiguration configuration) : this(
        sqsClient,
        configuration,
        o => JsonSerializer.Serialize(o),
        type => type.Name)
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
        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = configuration.QueueUrl,
            MessageBody = serializer(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    TypeAttribute,
                    new MessageAttributeValue { StringValue = typeMapper(typeof(TM)), DataType = "String" }
                }
            }
        });
    }

    public void Dispose()
    {
        sqsClient.Dispose();
    }
}