using System.Text.Json;
using CO.CDP.MQ.Pull;
using static CO.CDP.Organisation.WebApi.Events.EventDeserializer.EventDeserializerException;

namespace CO.CDP.Organisation.WebApi.Events;

public static class EventDeserializer
{
    public static Deserializer Deserializer => (type, body) =>
    {
        if (type == "PponGenerated")
        {
            return Deserialize<PponGenerated>(type, body);
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

    public class EventDeserializerException(string message, Exception? cause = null)
        : Exception(message, cause)
    {
        public class UnknownEventException(string message, Exception? cause = null)
            : EventDeserializerException(message, cause);

        public class DeserializationFailedException(string message, Exception? cause = null)
            : EventDeserializerException(message, cause);
    }
}