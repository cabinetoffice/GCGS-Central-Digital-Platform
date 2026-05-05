using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;

namespace CO.CDP.UserManagement.App.Tests.Application.Removal;

public class UserClaimsTests
{
    [Fact]
    public void GetOrganisationRole_WhenOrgNotInClaims_ReturnsNull()
    {
        var claims = MakeClaims();

        var result = claims.GetOrganisationRole(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("Owner", OrganisationRole.Owner)]
    [InlineData("Admin", OrganisationRole.Admin)]
    [InlineData("Member", OrganisationRole.Member)]
    [InlineData("owner", OrganisationRole.Owner)]   // case-insensitive
    [InlineData("ADMIN", OrganisationRole.Admin)]
    public void GetOrganisationRole_WhenOrgPresent_ParsesRoleCorrectly(string roleString, OrganisationRole expected)
    {
        var orgId = Guid.NewGuid();
        var claims = MakeClaims(orgId, roleString);

        var result = claims.GetOrganisationRole(orgId);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetOrganisationRole_WhenRoleStringUnrecognised_ReturnsNull()
    {
        var orgId = Guid.NewGuid();
        var claims = MakeClaims(orgId, "UnknownRole");

        var result = claims.GetOrganisationRole(orgId);

        result.Should().BeNull();
    }

    [Fact]
    public void GetOrganisationRole_WhenMultipleOrgs_ReturnsRoleForCorrectOrg()
    {
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();
        var claims = new UserClaims
        {
            UserPrincipalId = "urn:test",
            Organisations = new List<OrganisationMembershipClaim>
            {
                new() { OrganisationId = orgA, OrganisationName = "A", OrganisationRole = "Owner" },
                new() { OrganisationId = orgB, OrganisationName = "B", OrganisationRole = "Member" }
            }
        };

        claims.GetOrganisationRole(orgA).Should().Be(OrganisationRole.Owner);
        claims.GetOrganisationRole(orgB).Should().Be(OrganisationRole.Member);
    }

    private static UserClaims MakeClaims(Guid? orgId = null, string? roleString = null) =>
        new()
        {
            UserPrincipalId = "urn:test",
            Organisations = orgId.HasValue
                ? new List<OrganisationMembershipClaim>
                {
                    new() { OrganisationId = orgId.Value, OrganisationName = "Test Org", OrganisationRole = roleString! }
                }
                : new List<OrganisationMembershipClaim>()
        };
}
