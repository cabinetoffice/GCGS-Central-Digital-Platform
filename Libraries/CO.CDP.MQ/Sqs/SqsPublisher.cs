using Amazon.SQS;
using Amazon.SQS.Model;

namespace CO.CDP.MQ.Sqs;

public class SqsPublisher(AmazonSQSClient sqsClient, Func<Type, string> messageRouter, Func<object, string> serializer) : IPublisher
{
    public void Publish<TM>(TM message) where TM : notnull
    {
        sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = messageRouter(typeof(TM)),
            MessageBody = serializer(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "type", new MessageAttributeValue { StringValue = typeof(TM).Name, DataType = "String" } }
            }
        });
    }

    public void Dispose()
    {
        sqsClient.Dispose();
    }
}