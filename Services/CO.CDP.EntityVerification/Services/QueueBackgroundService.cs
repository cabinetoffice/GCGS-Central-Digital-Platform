using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace CO.CDP.EntityVerification.Services;

public class QueueBackgroundService : BackgroundService
{
    private readonly IRequestListener _listener;
    private readonly IConfiguration _config;
    private readonly ILogger<IRequestListener> _logger;

    public delegate void EvEventHandler(Message msg);
    public event EvEventHandler? OnNewMessage;

    public QueueBackgroundService(IRequestListener listener,
        IConfiguration config,
        ILogger<IRequestListener> logger)
    {
        _listener = listener;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        _listener.Register(this);
        
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
                MessageAttributeNames = ["TypeOfMessage"]
            };

            var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach (var message in response.Messages)
            {
                _logger.LogInformation("Received message: {messageBody}", message.Body);

                if (OnNewMessage != null)
                {
                    OnNewMessage(message);
                }

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