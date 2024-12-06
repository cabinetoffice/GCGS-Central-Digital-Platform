using System.Net;
using System.Net.Sockets;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace healthCheck.HealthChecks
{
    public class SqsHealthCheck : IHealthCheck
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly Dictionary<string, string> _queueUrls;

        public SqsHealthCheck(IAmazonSQS sqsClient, Dictionary<string, string> queueUrls)
        {
            _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            _queueUrls = queueUrls ?? throw new ArgumentNullException(nameof(queueUrls));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var details = new Dictionary<string, object>();

            foreach (var queue in _queueUrls)
            {
                var mainQueueUrl = queue.Value;
                var deadLetterQueueUrl = mainQueueUrl?.Replace(".fifo", "-deadletter.fifo");

                // Stage 1: DNS Resolution
                try
                {
                    var host = new Uri(mainQueueUrl).Host;
                    var addresses = await Dns.GetHostAddressesAsync(host);
                    details[$"{queue.Key} DNS Resolution"] = addresses.Length > 0 ? "Success" : "Failed";
                }
                catch (Exception ex)
                {
                    details[$"{queue.Key} DNS Resolution"] = $"Failed: {ex.Message}";
                    continue; // Skip further checks for this queue if DNS fails
                }

                // Stage 2: Connectivity Check
                try
                {
                    var host = new Uri(mainQueueUrl).Host;
                    using var tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(host, 443); // Assuming HTTPS for SQS
                    details[$"{queue.Key} Port Connectivity"] = "Success";
                }
                catch (Exception ex)
                {
                    details[$"{queue.Key} Port Connectivity"] = $"Failed: {ex.Message}";
                    continue;
                }

                // Stage 3: SQS Attributes Check
                try
                {
                    var mainQueueResponse = await _sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest
                    {
                        QueueUrl = mainQueueUrl,
                        AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
                    }, cancellationToken);

                    details[$"{queue.Key} Queue"] = $"Messages: {mainQueueResponse.ApproximateNumberOfMessages}";
                }
                catch (Exception ex)
                {
                    details[$"{queue.Key} Queue"] = $"Failed: {ex.Message}";
                }

                try
                {
                    var deadLetterResponse = await _sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest
                    {
                        QueueUrl = deadLetterQueueUrl,
                        AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
                    }, cancellationToken);

                    details[$"{queue.Key} Dead Letter Queue"] = $"Messages: {deadLetterResponse.ApproximateNumberOfMessages}";
                }
                catch (Exception ex)
                {
                    details[$"{queue.Key} Dead Letter Queue"] = $"Failed: {ex.Message}";
                }
            }

            return details.Count > 0
                ? HealthCheckResult.Healthy("SQS is accessible", details)
                : HealthCheckResult.Unhealthy("SQS is unavailable", null, details);
        }
    }
}
