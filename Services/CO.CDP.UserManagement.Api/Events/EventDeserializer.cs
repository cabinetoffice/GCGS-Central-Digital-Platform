using System.Text.Json;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Infrastructure.Events;
using InvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Api.Events;

public static class EventDeserializer
{
    public static readonly Deserializer Deserializer = (type, body) =>
        type switch
        {
            nameof(OrganisationRegistered) => Deserialize<OrganisationRegistered>(type, body),
            nameof(OrganisationUpdated) => Deserialize<OrganisationUpdated>(type, body),
            nameof(PersonRemovedFromOrganisation) => Deserialize<PersonRemovedFromOrganisation>(type, body),
            nameof(PersonScopesUpdated) => Deserialize<PersonScopesUpdated>(type, body),
            nameof(PersonInviteClaimed) => Deserialize<PersonInviteClaimed>(type, body),
            _ => throw new UnknownEventException($"Unknown event type '{type}'")
        };

    private static TM Deserialize<TM>(string type, string body) =>
        JsonSerializer.Deserialize<TM>(body)
        ?? throw new InvalidOperationException($"Deserialized null for type '{type}' from: {body}");
}