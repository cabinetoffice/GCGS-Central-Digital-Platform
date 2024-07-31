using CO.CDP.EntityVerification.Events;
using FluentAssertions;
using System.Text.Json;
using static CO.CDP.EntityVerification.Events.EventDeserializer.EventDeserializerException;
using static CO.CDP.EntityVerification.Tests.Events.EventsFactories;

namespace CO.CDP.EntityVerification.Tests.Events;

public class EventDeserializerTest
{
    [Fact]
    public void ItDeserializesOrganisationRegisteredEvent()
    {
        var organisationRegistered = GivenOrganisationRegisteredEvent();
        var serialized = JsonSerializer.Serialize(organisationRegistered);

        var deserialized = EventDeserializer.Deserializer("OrganisationRegistered", serialized);

        deserialized.Should().BeEquivalentTo(organisationRegistered);
    }

    [Fact]
    public void ItDeserializesOrganisationUpdateEvent()
    {
        var organisationUpdate = GivenOrganisationUpdatedEvent();
        var serialized = JsonSerializer.Serialize(organisationUpdate);

        var deserialized = EventDeserializer.Deserializer("OrganisationUpdated", serialized);

        deserialized.Should().BeEquivalentTo(organisationUpdate);
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