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
        private readonly string? _OrganisationQueueUrl;
        private readonly string? _EntityVerificationQueueUrl;
        private readonly string? _OrganisationDeadLetterQueueUrl;
        private readonly string? _EntityVerificationDeadLetterQueueUrl;

        public SqsController(IAmazonSQS sqsClient, IConfiguration configuration)
        {
            _sqsClient = sqsClient;
            _OrganisationQueueUrl = configuration["QUEUE_URL_ORGANISATION"];
            _EntityVerificationQueueUrl = configuration["QUEUE_URL_ENTITY_VERIFICATION"];
            _OrganisationDeadLetterQueueUrl = _OrganisationQueueUrl?.Replace(".fifo", "-deadletter.fifo");
            _EntityVerificationDeadLetterQueueUrl = _EntityVerificationQueueUrl?.Replace(".fifo", "-deadletter.fifo");
        }

        private string GetQueueUrl(QueueNames queue)
        {
            return queue switch
            {
                QueueNames.OrganisationQueue => _OrganisationQueueUrl ?? throw new InvalidOperationException("Organisation queue URL not configured."),
                QueueNames.EntityVerificationQueue => _EntityVerificationQueueUrl ?? throw new InvalidOperationException("EntityVerification queue URL not configured."),
                QueueNames.OrganisationDeadLetterQueue => _OrganisationDeadLetterQueueUrl ?? throw new InvalidOperationException("Organisation dead-letter queue URL not configured."),
                QueueNames.EntityVerificationDeadLetterQueue => _EntityVerificationDeadLetterQueueUrl ?? throw new InvalidOperationException("EntityVerification dead-letter queue URL not configured."),
                _ => throw new ArgumentOutOfRangeException(nameof(queue), queue, null),
            };
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromQuery] string message, [FromQuery] QueueNames queue, [FromQuery] string? messageGroupId = "health-check-test-group")
        {
            if (queue == QueueNames.OrganisationDeadLetterQueue || queue == QueueNames.EntityVerificationDeadLetterQueue)
            {
                return BadRequest(new { Status = "Error", Message = "Cannot send messages directly to dead-letter queues." });
            }

            var queueUrl = GetQueueUrl(queue);
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message,
                MessageGroupId = messageGroupId ?? "health-check-test-group"
            };

            var response = await _sqsClient.SendMessageAsync(sendMessageRequest);

            return Ok(new { QueueName = queue.ToString(), QueueUrl = queueUrl, Status = "Success", MessageId = response.MessageId });
        }

        [HttpGet("consume")]
        public async Task<IActionResult> ConsumeMessages([FromQuery] QueueNames queue, [FromQuery] string? messageGroupId = null)
        {
            var queueUrl = GetQueueUrl(queue);
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

            return Ok(new { QueueName = queue.ToString(), QueueUrl = queueUrl, Messages = messages });
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestQueues()
        {
            var testResults = new List<object>();

            foreach (QueueNames queue in Enum.GetValues(typeof(QueueNames)))
            {
                try
                {
                    var queueUrl = GetQueueUrl(queue);
                    var sendMessageRequest = new SendMessageRequest
                    {
                        QueueUrl = queueUrl,
                        MessageBody = "Test message",
                        MessageGroupId = "health-check-test-group",
                        MessageDeduplicationId = Guid.NewGuid().ToString() // Ensure deduplication ID is provided
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
