using System.Text.Json;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Infrastructure.Events;
using FluentAssertions;

namespace CO.CDP.UserManagement.Api.Tests.Events;

public class EventDeserializerTests
{
    [Fact]
    public void Deserializer_DeserializesOrganisationRegistered()
    {
        var json = JsonSerializer.Serialize(new
        {
            Id = "e4e5e6e7-0000-0000-0000-000000000001",
            Name = "Acme Ltd",
            Roles = new[] { "supplier" },
            Type = 0,
            FounderPersonId = (Guid?)Guid.NewGuid(),
            FounderUserUrn = "urn:fdc:gov.uk:2022:user1"
        });

        var result = EventDeserializer.Deserializer("OrganisationRegistered", json);

        result.Should().BeOfType<OrganisationRegistered>();
        var evt = (OrganisationRegistered)result;
        evt.Name.Should().Be("Acme Ltd");
        evt.Roles.Should().Contain("supplier");
        evt.FounderUserUrn.Should().Be("urn:fdc:gov.uk:2022:user1");
    }

    [Fact]
    public void Deserializer_DeserializesOrganisationUpdated()
    {
        var json = JsonSerializer.Serialize(new { Id = "abc", Name = "Updated Name" });
        var result = EventDeserializer.Deserializer("OrganisationUpdated", json);
        result.Should().BeOfType<OrganisationUpdated>();
        ((OrganisationUpdated)result).Name.Should().Be("Updated Name");
    }

    [Fact]
    public void Deserializer_DeserializesPersonRemovedFromOrganisation()
    {
        var json = JsonSerializer.Serialize(new
        {
            OrganisationId = Guid.NewGuid().ToString(),
            PersonId = Guid.NewGuid().ToString()
        });
        var result = EventDeserializer.Deserializer("PersonRemovedFromOrganisation", json);
        result.Should().BeOfType<PersonRemovedFromOrganisation>();
    }

    [Fact]
    public void Deserializer_DeserializesPersonScopesUpdated()
    {
        var json = JsonSerializer.Serialize(new
        {
            OrganisationId = Guid.NewGuid().ToString(),
            PersonId = Guid.NewGuid().ToString(),
            Scopes = new[] { "ADMIN" },
        });
        var result = EventDeserializer.Deserializer("PersonScopesUpdated", json);
        result.Should().BeOfType<PersonScopesUpdated>();
    }

    [Fact]
    public void Deserializer_DeserializesPersonInviteClaimed()
    {
        var json = JsonSerializer.Serialize(new
        {
            OrganisationId = Guid.NewGuid().ToString(),
            PersonId = Guid.NewGuid().ToString(),
            UserPrincipalId = "urn:user:1",
            Scopes = new[] { "EDITOR" },
        });
        var result = EventDeserializer.Deserializer("PersonInviteClaimed", json);
        result.Should().BeOfType<PersonInviteClaimed>();
    }

    [Fact]
    public void Deserializer_ThrowsForUnknownEventType()
    {
        var act = () => EventDeserializer.Deserializer("SomethingRandom", "{}");
        act.Should().Throw<UnknownEventException>()
            .WithMessage("*SomethingRandom*");
    }
}