using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.MQ;
using CO.CDP.MQ.Tests;

namespace CO.CDP.AwsServices.Tests.Sqs;

public class SqsDispatcherTest : DispatcherContractTest, IClassFixture<LocalStackFixture>
{
    private readonly LocalStackFixture _localStack;
    private readonly AmazonSQSClient _sqsClient;

    public SqsDispatcherTest(LocalStackFixture localStack)
    {
        _localStack = localStack;
        _sqsClient = SqsClient();
    }

    protected override async Task PublishMessage<TM>(TM message)
    {
        var queue = await _sqsClient.GetQueueUrlAsync(TestQueue);
        await _sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queue.QueueUrl,
            MessageBody = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                { "Type", new MessageAttributeValue { StringValue = typeof(TM).Name, DataType = "String" } }
            }
        });
    }

    protected override async Task<IDispatcher> CreateDispatcher()
    {
        var queue = await _sqsClient.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = TestQueue,
            Attributes =
            {
                { "VisibilityTimeout", "0" }
            }
        });
        return new SqsDispatcher(SqsClient(),
            new SqsDispatcherConfiguration
            {
                MaxNumberOfMessages = 1,
                QueueUrl = queue.QueueUrl,
                WaitTimeSeconds = 1
            },
            (type, body) =>
            {
                if (type == "TestMessage")
                {
                    return JsonSerializer.Deserialize<TestMessage>(body) ??
                           throw new Exception(
                               $"Could not deserialize type `{type}` from body `{body}`.");
                }

                throw new Exception($"Could not deserialize type `{type}` from body `{body}`.");
            },
            (type, typeName) => type.Name == typeName);
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

    protected override async Task<int> CountMessagesInQueue()
    {
        var queue = await _sqsClient.GetQueueUrlAsync(TestQueue);
        var response = await _sqsClient.ReceiveMessageAsync(queue.QueueUrl);
        return response.Messages.Count;
    }
}