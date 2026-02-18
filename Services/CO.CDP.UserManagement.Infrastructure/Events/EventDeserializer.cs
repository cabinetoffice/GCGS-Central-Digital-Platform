using System.Text.Json;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.UserManagement.Core.Exceptions;

namespace CO.CDP.UserManagement.Infrastructure.Events;

/// <summary>
/// Deserializes SQS event messages into strongly-typed event objects.
/// </summary>
public static class EventDeserializer
{
    public static Deserializer Deserializer => (type, body) =>
    {
        if (type == "OrganisationRegistered")
        {
            return Deserialize<OrganisationRegistered>(type, body);
        }

        if (type == "OrganisationUpdated")
        {
            return Deserialize<OrganisationUpdated>(type, body);
        }

        if (type == "PersonInviteClaimed")
        {
            return Deserialize<PersonInviteClaimed>(type, body);
        }

        throw new UnknownEventException($"Unrecognised type `{type}` for event `{body}`.");
    };

    private static TM Deserialize<TM>(string type, string body)
    {
        try
        {
            return JsonSerializer.Deserialize<TM>(body) ??
                   throw new DeserializationFailedException(
                       $"Could not deserialize type `{type}` from event `{body}`.");
        }
        catch (JsonException cause)
        {
            throw new DeserializationFailedException($"Could not deserialize type `{type}` from event `{body}`.",
                cause);
        }
    }


}
