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
        var organisationRegistered = GvienOrganisationRegisteredEvent();
        var serialized = JsonSerializer.Serialize(organisationRegistered);

        var deserialized = EventDeserializer.Deserializer("OrganisationRegistered", serialized);

        deserialized.Should().BeEquivalentTo(organisationRegistered);
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

    private static OrganisationRegistered GvienOrganisationRegisteredEvent() =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Acme Ltd",
            Identifier = new Identifier
            {
                Id = "93433423432",
                LegalName = "Acme Ltd",
                Scheme = "GB-COH",
                Uri = null
            },
            AdditionalIdentifiers =
            [
                new Identifier
                {
                    Id = "GB123123123",
                    LegalName = "Acme Ltd",
                    Scheme = "VAT",
                    Uri = null
                }
            ],
            Roles = ["supplier"]
        };
}