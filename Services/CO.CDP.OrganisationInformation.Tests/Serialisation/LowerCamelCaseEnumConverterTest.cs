using System.Text.Json;
using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Tests.Serialisation;

public class LowerCamelCaseEnumConverterTest
{
    [Fact]
    public void Serialize_ShouldReadSinglePartyRoleEnum_WhenProvidedSerialisedLowerCamelCasedPartyRoles()
    {
        var jsonString = JsonSerializer.Serialize(new List<PartyRole>() { PartyRole.Supplier });

        jsonString.Should().BeEquivalentTo("[\"supplier\"]");
    }

    [Fact]
    public void Deserialize_ShouldSerialiseSinglePartyRole_WhenProvidedPartyRolesEnum()
    {
        var partyRole = JsonSerializer.Deserialize<List<PartyRole>>("[\"supplier\"]");

        partyRole.Should().BeEquivalentTo(new List<PartyRole>() { PartyRole.Supplier });
    }

    [Fact]
    public void Serialize_ShouldReadMultiplePartyRolesEnum_WhenProvidedSerialisedLowerCamelCasedPartyRoles()
    {
        var jsonString = JsonSerializer.Serialize(new List<PartyRole> { PartyRole.Supplier, PartyRole.ProcuringEntity });

        jsonString.Should().BeEquivalentTo("[\"supplier\",\"procuringEntity\"]");
    }

    [Fact]
    public void Deserialize_ShouldSerialiseMultiplePartyRoles_WhenProvidedPartyRolesEnum()
    {
        var partyRoles = JsonSerializer.Deserialize<List<PartyRole>>("[\"supplier\",\"procuringEntity\"]");

        partyRoles.Should().BeEquivalentTo(new List<PartyRole> { PartyRole.Supplier, PartyRole.ProcuringEntity });
    }
}