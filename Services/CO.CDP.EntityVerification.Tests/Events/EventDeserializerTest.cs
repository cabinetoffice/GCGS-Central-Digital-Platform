using System.Text.Json;
using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Tests.Ppon;
using FluentAssertions;
using static CO.CDP.EntityVerification.Events.EventDeserializer.EventDeserializerException;

namespace CO.CDP.EntityVerification.Tests.Events;

public class EventDeserializerTest
{
    [Fact]
    public void ItDeserializesOrganisationRegisteredEvent()
    {
        var organisationRegistered = EventsFactories.GivenOrganisationRegisteredEvent();
        var serialized = JsonSerializer.Serialize(organisationRegistered);

        var deserialized = EventDeserializer.Deserializer("OrganisationRegistered", serialized);

        deserialized.Should().BeEquivalentTo(organisationRegistered);
    }

    [Fact]
    public void ItDeserializesOrganisationUpdateEvent()
    {
        var organisationUpdate = EventsFactories.GivenOrganisationUpdatedEvent();
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