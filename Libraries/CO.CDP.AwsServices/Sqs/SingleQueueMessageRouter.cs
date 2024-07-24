using Amazon.SQS;

namespace CO.CDP.AwsServices.Sqs;

public class SingleQueueMessageRouter(IAmazonSQS sqsClient, string queueName)
{
    private string? _queueUrl;

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