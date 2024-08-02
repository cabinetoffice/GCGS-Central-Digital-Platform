using Microsoft.AspNetCore.Mvc;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Threading.Tasks;
using healthCheck.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace healthCheck.Controllers
{
    [ApiController]
    [Route("sqs")]
    public class SqsController : ControllerBase
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string? _healthCheckQueueUrl;
        private readonly string? _healthCheckDeadLetterQueueUrl;

        public SqsController(IAmazonSQS sqsClient, IConfiguration configuration)
        {
            _sqsClient = sqsClient;
            _healthCheckQueueUrl = configuration["QUEUE_URL_HEALTHCHECK"];
            _healthCheckDeadLetterQueueUrl = _healthCheckQueueUrl?.Replace(".fifo", "-deadletter.fifo");
        }

        private string GetQueueUrl(QueueNames queue)
        {
            return queue switch
            {
                QueueNames.HealthCheckQueue => _healthCheckQueueUrl ?? throw new InvalidOperationException("Health check queue URL not configured."),
                QueueNames.HealthCheckDeadLetterQueue => _healthCheckDeadLetterQueueUrl ?? throw new InvalidOperationException("Health check dead-letter queue URL not configured."),
                _ => throw new ArgumentOutOfRangeException(nameof(queue), queue, null),
            };
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromQuery] string message, [FromQuery] string? messageGroupId = "health-check-test-group")
        {
            var queueUrl = GetQueueUrl(QueueNames.HealthCheckQueue);
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message,
                MessageGroupId = messageGroupId ?? "health-check-test-group"
            };

            var response = await _sqsClient.SendMessageAsync(sendMessageRequest);

            return Ok(new { QueueName = "HealthCheckQueue", QueueUrl = queueUrl, Status = "Success", MessageId = response.MessageId });
        }

        [HttpGet("consume")]
        public async Task<IActionResult> ConsumeMessages([FromQuery] string? messageGroupId = null)
        {
            var queueUrl = GetQueueUrl(QueueNames.HealthCheckQueue);
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                MessageAttributeNames = new List<string> { "All" }
            };

            if (!string.IsNullOrEmpty(messageGroupId))
            {
                receiveMessageRequest.MessageAttributeNames.Add("MessageGroupId");
            }

            var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

            var messages = string.IsNullOrEmpty(messageGroupId)
                ? response.Messages
                : response.Messages.FindAll(msg => msg.Attributes["MessageGroupId"] == messageGroupId);

            return Ok(new { QueueName = "HealthCheckQueue", QueueUrl = queueUrl, Messages = messages });
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestQueues()
        {
            var testResults = new List<object>();
            var queues = new QueueNames[] { QueueNames.HealthCheckQueue, QueueNames.HealthCheckDeadLetterQueue };

            foreach (var queue in queues)
            {
                try
                {
                    var queueUrl = GetQueueUrl(queue);
                    var sendMessageRequest = new SendMessageRequest
                    {
                        QueueUrl = queueUrl,
                        MessageBody = "Test message",
                        MessageGroupId = "health-check-test-group",
                        MessageDeduplicationId = Guid.NewGuid().ToString()
                    };

                    var response = await _sqsClient.SendMessageAsync(sendMessageRequest);
                    testResults.Add(new { QueueName = queue.ToString(), QueueUrl = queueUrl, Status = "Success", MessageId = response.MessageId });
                }
                catch (Exception ex)
                {
                    testResults.Add(new { QueueName = queue.ToString(), Status = "Error", Message = ex.Message });
                }
            }

            return Ok(testResults);
        }
    }
}
