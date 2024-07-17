using System.Text.Json;
using CO.CDP.EntityVerification.Events;
using FluentAssertions;
using static CO.CDP.EntityVerification.Events.EventDeserializer.EventDeserializerException;

namespace CO.CDP.EntityVerification.Tests.Events;

public class EventDeserializerTest
{
    [Fact]
    public void ItDeserializesOrganisationRegisteredEvent()
    {
        var organisationRegistered = new OrganisationRegistered
        {
            Name = "Acme Ltd"
        };
        var serialized = JsonSerializer.Serialize(organisationRegistered);

        var deserialized = EventDeserializer.Deserializer("OrganisationRegistered", serialized);

        deserialized.Should().Be(organisationRegistered);
    }

    [Fact]
    public void ItThrowsAnExceptionIfTheEventIsUnknown()
    {
        var action = () => EventDeserializer.Deserializer("UnknownEvent", "{}");

        action.Should().Throw<UnknownEventException>();
    }

    [Fact]
    public void ItThrowsAnExceptionIfTheEventCannotBeDeserialized()
    {
        var action = () => EventDeserializer.Deserializer("OrganisationRegistered", "{}");

        action.Should().Throw<DeserializationFailedException>();
    }
}