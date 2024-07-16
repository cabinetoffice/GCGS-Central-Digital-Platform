using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using FluentAssertions;

namespace CO.CDP.MQ.Tests;

public class LearnSqsTest(LocalStackFixture localStack) : IClassFixture<LocalStackFixture>
{
    [Fact(Skip = "This is a learning test that does not cover production code.")]
    public async Task ItCreatesSqsQueue()
    {
        var sqsClient = SqsClient();

        var createQueueResponse = await sqsClient.CreateQueueAsync("queue-1");
        var createdQueueUrl = createQueueResponse.QueueUrl;
        var foundQueue = await sqsClient.GetQueueUrlAsync("queue-1");

        foundQueue.QueueUrl.Should().Be(createdQueueUrl);
    }

    [Fact(Skip = "This is a learning test that does not cover production code.")]
    public async Task ItListensToPublishedMessages()
    {
        var sqsClient = SqsClient();

        var createdQueue = await sqsClient.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = "queue-2"
        });

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = createdQueue.QueueUrl,
            MessageBody = "42",
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "type", new MessageAttributeValue { StringValue = "int", DataType = "String"} }
            }
        });

        var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = createdQueue.QueueUrl,
            MessageAttributeNames = ["type"]
        });

        response.Messages.Should().HaveCount(1);

        var message = response.Messages.First();

        message.Body.Should().Be("42");
        message.MessageAttributes.Should()
            .Contain(d => d.Key == "type" && d.Value.StringValue == "int" );

        var deleteMessageRequest = new DeleteMessageRequest
        {
            QueueUrl = createdQueue.QueueUrl,
            ReceiptHandle = message.ReceiptHandle
        };
        await sqsClient.DeleteMessageAsync(deleteMessageRequest);
    }

    private AmazonSQSClient SqsClient()
    {
        var config = new AmazonSQSConfig
        {
            ServiceURL = localStack.ConnectionString,
            UseHttp = false,
            AuthenticationRegion = "eu-west-1"
        };

        var credentials = new BasicAWSCredentials("test", "test");
        var sqsClient = new AmazonSQSClient(credentials, config);
        return sqsClient;
    }
}