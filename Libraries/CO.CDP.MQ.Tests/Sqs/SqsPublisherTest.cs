using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.MQ.Sqs;
using FluentAssertions;

namespace CO.CDP.MQ.Tests.Sqs;

public class SqsPublisherTest : PublisherContractTest, IClassFixture<LocalStackFixture>
{
    private readonly LocalStackFixture _localStack;
    private readonly AmazonSQSClient _sqsClient;

    public SqsPublisherTest(LocalStackFixture localStack)
    {
        _localStack = localStack;
        _sqsClient = SqsClient();
    }

    protected override async Task<T> waitForOneMessage<T>() where T : class
    {
        var sqsClient = SqsClient();
        var queue = await sqsClient.GetQueueUrlAsync("test-queue-1");
        var messages = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queue.QueueUrl,
            MaxNumberOfMessages = 1,
            MessageAttributeNames = ["type"]
        });
        var message = messages.Messages.First<Message>();
        var type = message.MessageAttributes.GetValueOrDefault("type")?.StringValue;
        type.Should().Be(typeof(T).Name);
        return JsonSerializer.Deserialize<T>(message.Body) ??
               throw new Exception($"Unable to deserialize {message.Body} into {typeof(T).FullName}");
    }

    protected override async Task<IPublisher> CreatePublisher()
    {
        var sqsClient = SqsClient();
        var queue = await _sqsClient.CreateQueueAsync(new CreateQueueRequest { QueueName = "test-queue-1" });
        var queueUrl = queue.QueueUrl;
        return new SqsPublisher(sqsClient, _ => queueUrl, o => JsonSerializer.Serialize(o));
    }

    private AmazonSQSClient SqsClient()
    {
        var config = new AmazonSQSConfig
        {
            ServiceURL = _localStack.ConnectionString,
            UseHttp = false,
            AuthenticationRegion = "eu-west-1"
        };

        var credentials = new BasicAWSCredentials("test", "test");
        var sqsClient = new AmazonSQSClient(credentials, config);
        return sqsClient;
    }
}