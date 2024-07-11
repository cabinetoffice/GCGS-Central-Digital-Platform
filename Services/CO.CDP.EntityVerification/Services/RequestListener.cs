using Amazon.SQS.Model;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.SQS;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace CO.CDP.EntityVerification.Services;

public class RequestListener : IRequestListener
{
    private readonly ILogger<RequestListener> _logger;
    private readonly OrganisationRegisteredEvent _newOrgProcessor;

    private const string OrganisationRegisteredMessageType = "OrganisationRegistered";

    public RequestListener(OrganisationRegisteredEvent newOrgProcessor, ILogger<RequestListener> logger)
    {
        _newOrgProcessor = newOrgProcessor;
        _logger = logger;
    }

    public void Register(IQueueProcessor messageReceiver)
    {
        messageReceiver.OnNewMessage += NewMessage;
    }

    void NewMessage(Message msg)
    {
        _logger.LogInformation("RequestListener: New Message Received.");

        if (msg.MessageAttributes.ContainsKey("TypeOfMessage"))
        {
            if (msg.MessageAttributes["TypeOfMessage"].StringValue == OrganisationRegisteredMessageType)
            {
                var newOrg = JsonSerializer.Deserialize<OrganisationRegisteredMessage>(msg.Body);
                _newOrgProcessor.Action(newOrg);
            }
        }
    }
}
