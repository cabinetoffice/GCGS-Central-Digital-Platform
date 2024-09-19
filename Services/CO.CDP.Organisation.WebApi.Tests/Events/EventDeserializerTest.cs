using System.Text.Json;
using CO.CDP.Organisation.WebApi.Events;
using FluentAssertions;

namespace CO.CDP.Organisation.WebApi.Tests.Events;

public class EventDeserializerTest
{
    [Fact]
    public void ItDeserializesOrganisationRegisteredEvent()
    {
        var pponGenerated = new PponGenerated
        {
            OrganisationId = Guid.Parse("58efbd0f-37bd-44e0-baa4-bcae42ab0567"),
            Id = "1234453",
            Scheme = "GB-PPON",
            LegalName = "Acme Ltd"
        };
        var serialized = JsonSerializer.Serialize(pponGenerated);

        var deserialized = EventDeserializer.Deserializer("PponGenerated", serialized);

        deserialized.Should().BeEquivalentTo(pponGenerated);
    }

    [Fact]
    public void ItThrowsAnExceptionIfTheEventIsUnknown()
    {
        var action = () => EventDeserializer.Deserializer("UnknownEvent", "{}");

        action.Should().Throw<EventDeserializer.EventDeserializerException.UnknownEventException>();
    }

    [Fact]
    public void ItThrowsAnExceptionIfTheEventCannotBeDeserialized()
    {
        var action = () => EventDeserializer.Deserializer("PponGenerated", "{}");

        action.Should().Throw<EventDeserializer.EventDeserializerException.DeserializationFailedException>();
    }
}