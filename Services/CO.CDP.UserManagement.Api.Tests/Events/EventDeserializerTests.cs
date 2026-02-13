using CO.CDP.UserManagement.Infrastructure.Events;
using FluentAssertions;
using System.Text.Json;
using CO.CDP.UserManagement.Core.Exceptions;

namespace CO.CDP.UserManagement.Api.Tests.Events;

public class EventDeserializerTests
{
    [Fact]
    public void Deserializer_OrganisationRegistered_DeserializesCorrectly()
    {
        var eventBody = JsonSerializer.Serialize(new
        {
            Id = "d2dab085-ec23-481c-b970-ee6b372f9f57",
            Name = "Test Organisation",
            Identifier = new
            {
                Id = "12345678",
                LegalName = "Test Organisation Ltd",
                Scheme = "GB-COH"
            },
            AdditionalIdentifiers = Array.Empty<object>(),
            Addresses = Array.Empty<object>(),
            ContactPoint = new
            {
                Name = "Contact Name",
                Email = "contact@example.com",
                Telephone = "01234567890"
            },
            Roles = new[] { "tenderer" },
            Type = "organisation"
        });

        var result = EventDeserializer.Deserializer("OrganisationRegistered", eventBody);

        result.Should().BeOfType<OrganisationRegistered>();
        var orgEvent = (OrganisationRegistered)result;
        orgEvent.Id.Should().Be("d2dab085-ec23-481c-b970-ee6b372f9f57");
        orgEvent.Name.Should().Be("Test Organisation");
    }

    [Fact]
    public void Deserializer_OrganisationUpdated_DeserializesCorrectly()
    {
        var eventBody = JsonSerializer.Serialize(new
        {
            Id = "a1b2c3d4-e5f6-4789-a1b2-c3d4e5f67890",
            Name = "Updated Organisation",
            Identifier = new
            {
                Id = "87654321",
                LegalName = "Updated Organisation Ltd",
                Scheme = "GB-COH"
            },
            AdditionalIdentifiers = Array.Empty<object>(),
            Addresses = Array.Empty<object>(),
            ContactPoint = new
            {
                Name = "Updated Contact",
                Email = "updated@example.com",
                Telephone = "09876543210"
            },
            Roles = new[] { "supplier" }
        });

        var result = EventDeserializer.Deserializer("OrganisationUpdated", eventBody);

        result.Should().BeOfType<OrganisationUpdated>();
        var orgEvent = (OrganisationUpdated)result;
        orgEvent.Id.Should().Be("a1b2c3d4-e5f6-4789-a1b2-c3d4e5f67890");
        orgEvent.Name.Should().Be("Updated Organisation");
    }

    [Fact]
    public void Deserializer_PersonInviteClaimed_DeserializesCorrectly()
    {
        var personInviteGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var orgGuid = Guid.NewGuid();

        var eventBody = JsonSerializer.Serialize(new
        {
            PersonInviteGuid = personInviteGuid,
            PersonGuid = personGuid,
            UserUrn = "urn:fdc:gov.uk:2022:user123",
            OrganisationGuid = orgGuid
        });

        var result = EventDeserializer.Deserializer("PersonInviteClaimed", eventBody);

        result.Should().BeOfType<PersonInviteClaimed>();
        var inviteEvent = (PersonInviteClaimed)result;
        inviteEvent.PersonInviteGuid.Should().Be(personInviteGuid);
        inviteEvent.PersonGuid.Should().Be(personGuid);
        inviteEvent.UserUrn.Should().Be("urn:fdc:gov.uk:2022:user123");
        inviteEvent.OrganisationGuid.Should().Be(orgGuid);
    }

    [Fact]
    public void Deserializer_UnknownType_ThrowsUnknownEventException()
    {
        var eventBody = "{}";

        var act = () => EventDeserializer.Deserializer("UnknownEventType", eventBody);

        act.Should().Throw<UnknownEventException>()
            .WithMessage("Unrecognised type `UnknownEventType`*");
    }

    [Fact]
    public void Deserializer_InvalidJson_ThrowsDeserializationFailedException()
    {
        var invalidJson = "{ invalid json }";

        var act = () => EventDeserializer.Deserializer("OrganisationRegistered", invalidJson);

        act.Should().Throw<DeserializationFailedException>()
            .WithMessage("Could not deserialize type*");
    }
}
