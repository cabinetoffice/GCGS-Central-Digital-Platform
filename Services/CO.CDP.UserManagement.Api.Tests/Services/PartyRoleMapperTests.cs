using CO.CDP.UserManagement.Core.Constants;
using CO.CDP.UserManagement.Infrastructure.Services;
using FluentAssertions;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class PartyRoleMapperTests
{
    [Theory]
    [InlineData("buyer", PartyRole.Buyer)]
    [InlineData("supplier", PartyRole.Supplier)]
    [InlineData("tenderer", PartyRole.Tenderer)]
    [InlineData("procuringentity", PartyRole.ProcuringEntity)]
    [InlineData("BUYER", PartyRole.Buyer)]
    [InlineData("Supplier", PartyRole.Supplier)]
    public void MapFromStrings_MapsKnownRoles(string input, PartyRole expected)
    {
        var result = PartyRoleMapper.MapFromStrings([input]);
        result.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void MapFromStrings_ThrowsForUnknownRole()
    {
        var act = () => PartyRoleMapper.MapFromStrings(["unknownrole"]);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MapFromStrings_ReturnsEmptyForEmptyInput()
    {
        var result = PartyRoleMapper.MapFromStrings([]);
        result.Should().BeEmpty();
    }

    [Fact]
    public void MapFromStrings_MapsMultipleRoles()
    {
        var result = PartyRoleMapper.MapFromStrings(["buyer", "supplier"]);
        result.Should().HaveCount(2).And.Contain(PartyRole.Buyer).And.Contain(PartyRole.Supplier);
    }
}