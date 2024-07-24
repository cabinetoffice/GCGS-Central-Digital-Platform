using System.Text.Json;

namespace CO.CDP.OrganisationInformation.Tests.Serialisation;

public class LowerCamelCaseEnumConverterTest
{
    private readonly string _serialisedSinglePartyRoleString = "[\"supplier\"]";
    private readonly string _serialisedMultiplePartyRolesString = "[\"supplier\",\"procuringEntity\"]";

    private readonly List<PartyRole> _singlePartyRoleList = [PartyRole.Supplier];
    private readonly List<PartyRole> _multiplePartyRolesList = [PartyRole.Supplier, PartyRole.ProcuringEntity];

    [Fact]
    public void Serialize_ShouldReadSinglePartyRoleEnum_WhenProvidedSerialisedLowerCamelCasedPartyRoles()
    {
        var jsonString = JsonSerializer.Serialize(_singlePartyRoleList);

        Assert.Equal(_serialisedSinglePartyRoleString, jsonString);
    }

    [Fact]
    public void Deserialize_ShouldSerialiseSinglePartyRole_WhenProvidedPartyRolesEnum()
    {
        var partyRole = JsonSerializer.Deserialize<List<PartyRole>>(_serialisedSinglePartyRoleString);

        Assert.Equal(_singlePartyRoleList, partyRole);
    }

    [Fact]
    public void Serialize_ShouldReadMultiplePartyRolesEnum_WhenProvidedSerialisedLowerCamelCasedPartyRoles()
    {
        var jsonString = JsonSerializer.Serialize(_multiplePartyRolesList);

        Assert.Equal(_serialisedMultiplePartyRolesString, jsonString);
    }

    [Fact]
    public void Deserialize_ShouldSerialiseMultiplePartyRoles_WhenProvidedPartyRolesEnum()
    {
        var partyRoles = JsonSerializer.Deserialize<List<PartyRole>>(_serialisedMultiplePartyRolesString);

        Assert.Equal(_multiplePartyRolesList, partyRoles);
    }
}