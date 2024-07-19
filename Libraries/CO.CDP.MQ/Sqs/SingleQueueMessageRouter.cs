using Amazon.SQS;

namespace CO.CDP.MQ.Sqs;

public class SingleQueueMessageRouter(AmazonSQSClient sqsClient, string queueName)
{
    private string? _queueUrl = null;

    public async Task<string> QueueUrl(Type type)
    {
        return _queueUrl ??= await GetQueueUrl();
    }

    private async Task<string> GetQueueUrl()
    {
        var queue = await sqsClient.GetQueueUrlAsync(queueName);
        return queue.QueueUrl;
    }
}