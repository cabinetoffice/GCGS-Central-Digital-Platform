using Amazon.SQS;
using Amazon.SQS.Model;

namespace CO.CDP.MQ.Sqs;

public delegate Task<string> MessageRouter(Type type);

public delegate string Serializer(object message);

public delegate string TypeMapper(Type type);

public class SqsPublisher(
    AmazonSQSClient sqsClient,
    MessageRouter messageRouter,
    Serializer serializer,
    TypeMapper typeMapper) : IPublisher
{
    private const string TypeAttribute = "Type";

    public SqsPublisher(AmazonSQSClient sqsClient, MessageRouter messageRouter, Serializer serializer) : this(
        sqsClient, messageRouter, serializer, type => type.Name)
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