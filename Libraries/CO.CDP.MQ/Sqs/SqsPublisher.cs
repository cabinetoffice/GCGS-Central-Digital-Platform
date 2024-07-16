using Amazon.SQS;
using Amazon.SQS.Model;

namespace CO.CDP.MQ.Sqs;

public class SqsPublisher(AmazonSQSClient sqsClient, Func<Type, string> messageRouter, Func<object, string> serializer, Func<Type, string> typeMapper) : IPublisher
{
    public SqsPublisher(AmazonSQSClient sqsClient, Func<Type, string> messageRouter,
        Func<object, string> serializer) : this(
        sqsClient, messageRouter, serializer, type => type.Name)
    {
    }

    public void Publish<TM>(TM message) where TM : notnull
    {
        sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = messageRouter(typeof(TM)),
            MessageBody = serializer(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "type", new MessageAttributeValue { StringValue = typeMapper(typeof(TM)), DataType = "String" } }
            }
        });
    }

    public void Dispose()
    {
        sqsClient.Dispose();
    }
}