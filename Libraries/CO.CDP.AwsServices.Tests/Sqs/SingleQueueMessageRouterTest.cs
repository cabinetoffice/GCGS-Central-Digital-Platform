using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.AwsServices.Sqs;
using FluentAssertions;

namespace CO.CDP.AwsServices.Tests.Sqs;

public class SingleQueueMessageRouterTest(LocalStackFixture localStack) : IClassFixture<LocalStackFixture>
{
    [Fact]
    public async Task ItRoutesAllMessageTypesToASingleQueue()
    {
        var queueUrl = await GivenQueueExists("test-queue-a");

        var router = new SingleQueueMessageRouter(SqsClient(), "test-queue-a");

        (await router.QueueUrl(typeof(Foo))).Should().Be(queueUrl);
        (await router.QueueUrl(typeof(Bar))).Should().Be(queueUrl);
    }

    private async Task<string> GivenQueueExists(string queueName)
    {
        var queue = await SqsClient().CreateQueueAsync(new CreateQueueRequest { QueueName = queueName });
        return queue.QueueUrl;
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

    private record Foo;

    private record Bar;
}