using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.EntityVerification.Services;
using static CO.CDP.EntityVerification.SQS.IQueueProcessor;

namespace CO.CDP.EntityVerification.SQS;

public class QueueProcessor : IQueueProcessor
{
    private readonly ILogger<IRequestListener> _logger;
    private readonly IConfiguration _config;

    public event EvEventHandler OnNewMessage;

    public QueueProcessor(IConfiguration config, ILogger<IRequestListener> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async void Start(CancellationToken stoppingToken)
    {
        var config = new AmazonSQSConfig
        {
            ServiceURL = _config["ServiceURL"],
            UseHttp = false, 
            AuthenticationRegion = _config["AuthenticationRegion"]
        };

        // TODO: replace credentials with correct implementation.
        var credentials = new BasicAWSCredentials("test", "test");

        var sqsClient = new AmazonSQSClient(credentials, config);
        var queueUrl = _config["QueueUrl"];

        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = _config.GetValue<int>("MaxNumberOfMessages"),
                WaitTimeSeconds = _config.GetValue<int>("WaitTimeSeconds"),
                MessageAttributeNames = new List<string> { "TypeOfMessage" }
            };

            var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest);

            foreach (var message in response.Messages)
            {
                _logger.LogInformation($"Received message: {message.Body}");

                OnNewMessage(message);

                // Delete the message from the queue after processing
                var deleteMessageRequest = new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = message.ReceiptHandle
                };
                await sqsClient.DeleteMessageAsync(deleteMessageRequest);
            }
        }
    }
}