using Amazon.SQS.Model;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Model;
using System.Text.Json;

namespace CO.CDP.EntityVerification.Services;

public class RequestListener : IRequestListener
{
    private readonly ILogger<RequestListener> _logger;
    private readonly OrganisationRegisteredEventHandler _newOrgProcessor;

    private const string OrganisationRegisteredMessageType = "OrganisationRegistered";

    public RequestListener(OrganisationRegisteredEventHandler newOrgProcessor, ILogger<RequestListener> logger)
    {
        _newOrgProcessor = newOrgProcessor;
        _logger = logger;
    }

    public void Register(QueueBackgroundService messageReceiver)
    {
        messageReceiver.OnNewMessage += NewMessage;
    }

    void NewMessage(Message msg)
    {
        _logger.LogInformation("New Message Received.");

        if (msg.MessageAttributes.TryGetValue("TypeOfMessage", out MessageAttributeValue? value))
        {
            if (value.StringValue == OrganisationRegisteredMessageType)
            {
                var newOrg = JsonSerializer.Deserialize<OrganisationRegisteredMessage>(msg.Body);
                _newOrgProcessor.Action(newOrg!);
            }
        }
    }
}
