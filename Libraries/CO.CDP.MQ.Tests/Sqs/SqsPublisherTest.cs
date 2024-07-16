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
    private const string TestQueue = "test-queue-1";

    public SqsPublisherTest(LocalStackFixture localStack)
    {
        _localStack = localStack;
        _sqsClient = SqsClient();
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
        return JsonSerializer.Deserialize<T>(message.Body) ??
               throw new Exception($"Unable to deserialize {message.Body} into {typeof(T).FullName}");
    }

    protected override async Task<IPublisher> CreatePublisher()
    {
        var queue = await _sqsClient.CreateQueueAsync(new CreateQueueRequest { QueueName = TestQueue });
        var queueUrl = queue.QueueUrl;
        return new SqsPublisher(_sqsClient, _ => queueUrl, o => JsonSerializer.Serialize(o));
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