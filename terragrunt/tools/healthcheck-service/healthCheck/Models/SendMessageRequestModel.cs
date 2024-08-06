using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;

namespace healthCheck.Models
{
    public class SendMessageRequestModel
    {
        [SwaggerSchema(Description = "The content of the message to be sent.")]
        public string Message { get; set; }

        [SwaggerSchema(Description = "The name of the queue.")]
        public QueueNames Queue { get; set; }

        [SwaggerSchema(Description = "Message group ID for FIFO queues")]
        [DefaultValue("health-check-test-group")]
        public string MessageGroupId { get; set; } = "health-check-test-group";
    }
}
