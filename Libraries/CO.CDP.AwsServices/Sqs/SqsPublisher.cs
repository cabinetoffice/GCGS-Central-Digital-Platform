using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ;

namespace CO.CDP.AwsServices.Sqs;

public delegate Task<string> MessageRouter(Type type);

public delegate string Serializer(object message);

public delegate string TypeMapper(Type type);

public class SqsPublisher(
    IAmazonSQS sqsClient,
    MessageRouter messageRouter,
    Serializer serializer,
    TypeMapper typeMapper) : IPublisher
{
    private const string TypeAttribute = "Type";

    public SqsPublisher(IAmazonSQS sqsClient, MessageRouter messageRouter) : this(
        sqsClient, messageRouter, o => JsonSerializer.Serialize(o), type => type.Name)
    {
    }

    public async Task Publish<TM>(TM message) where TM : notnull
    {
        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = await messageRouter(typeof(TM)),
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