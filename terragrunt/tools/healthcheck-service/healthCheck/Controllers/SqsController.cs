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
        private readonly string? _dispatcherQueueUrl;
        private readonly string? _publisherQueueUrl;

        public SqsController(IAmazonSQS sqsClient, IConfiguration configuration)
        {
            _sqsClient = sqsClient;
            _dispatcherQueueUrl = configuration["QUEUE_URL_ORGANISATION"];
            _publisherQueueUrl = configuration["QUEUE_URL_ENTITY_VERIFICATION"];
        }

        private string GetQueueUrl(QueueNames queue)
        {
            return queue switch
            {
                QueueNames.DispatcherQueue => _dispatcherQueueUrl ?? throw new InvalidOperationException("Dispatcher queue URL not configured."),
                QueueNames.PublisherQueue => _publisherQueueUrl ?? throw new InvalidOperationException("Publisher queue URL not configured."),
                _ => throw new ArgumentOutOfRangeException(nameof(queue), queue, null),
            };
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromQuery] string message, [FromQuery] QueueNames queue)
        {
            var queueUrl = GetQueueUrl(queue);
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message
            };

            var response = await _sqsClient.SendMessageAsync(sendMessageRequest);

            return Ok(new { QueueName = queue.ToString(), QueueUrl = queueUrl, Status = "Success", MessageId = response.MessageId });
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMessages([FromQuery] QueueNames queue)
        {
            var queueUrl = GetQueueUrl(queue);
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10
            };

            var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

            return Ok(new { QueueName = queue.ToString(), QueueUrl = queueUrl, Messages = response.Messages });
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
                        MessageBody = "Test message"
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
