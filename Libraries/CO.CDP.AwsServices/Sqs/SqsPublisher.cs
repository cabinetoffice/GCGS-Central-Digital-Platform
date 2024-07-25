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
    string queueUrl,
    Serializer serializer,
    TypeMapper typeMapper) : IPublisher
{
    private const string TypeAttribute = "Type";

    public SqsPublisher(IAmazonSQS sqsClient, IOptions<AwsConfiguration> configuration) : this(
        sqsClient,
        configuration.Value.SqsPublisher?.QueueUrl ?? "")
    {
    }

    public SqsPublisher(IAmazonSQS sqsClient, string queueUrl) : this(
        sqsClient,
        queueUrl,
        o => JsonSerializer.Serialize(o),
        type => type.Name)
    {
    }

    public async Task Publish<TM>(TM message) where TM : notnull
    {
        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
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