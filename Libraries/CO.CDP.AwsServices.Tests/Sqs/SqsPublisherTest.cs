using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.MQ;
using CO.CDP.MQ.Outbox;
using CO.CDP.MQ.Tests;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CO.CDP.AwsServices.Tests.Sqs;

public class SqsPublisherTest : PublisherContractTest, IClassFixture<LocalStackFixture>
{
    private readonly LocalStackFixture _localStack;
    private readonly AmazonSQSClient _sqsClient;
    private const string TestQueue = "test-queue-1.fifo";

    public SqsPublisherTest(LocalStackFixture localStack)
    {
        _localStack = localStack;
        _sqsClient = SqsClient();
    }

    [Fact]
    public async Task ItPublishesOutboxMessageToTheQueue()
    {
        var publisher = await CreatePublisher();

        await publisher.Publish(new OutboxMessage
        {
            Type = "TestMessage",
            Message = "{\"Id\":13,\"Name\":\"Hello!\"}"
        });

        var message = await waitForOneMessage<TestMessage>();

        message.Should().Be(new TestMessage(13, "Hello!"));
    }

    protected override async Task<T> waitForOneMessage<T>() where T : class
    {
        var queue = await _sqsClient.GetQueueUrlAsync(TestQueue);
        var messages = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queue.QueueUrl,
            MaxNumberOfMessages = 1,
            MessageAttributeNames = ["Type"]
        });
        var message = messages.Messages.First<Message>();
        var type = message.MessageAttributes.GetValueOrDefault("Type")?.StringValue;
        type.Should().Be(typeof(T).Name);
        await _sqsClient.DeleteMessageAsync(queue.QueueUrl, message.ReceiptHandle);
        return JsonSerializer.Deserialize<T>(message.Body) ??
               throw new Exception($"Unable to deserialize {message.Body} into {typeof(T).FullName}");
    }

    protected override async Task<IPublisher> CreatePublisher()
    {
        var queue = await _sqsClient.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = TestQueue,
            Attributes = new Dictionary<string, string>
            {
                { "FifoQueue", "true" },
                { "ContentBasedDeduplication", "true" }
            }
        });
        var queueUrl = queue.QueueUrl ?? "";
        return new SqsPublisher(
            _sqsClient,
            new SqsPublisherConfiguration { QueueUrl = queueUrl, MessageGroupId = "test-messages" },
            LoggerFactory.Create(_ => { }).CreateLogger<SqsPublisher>());
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