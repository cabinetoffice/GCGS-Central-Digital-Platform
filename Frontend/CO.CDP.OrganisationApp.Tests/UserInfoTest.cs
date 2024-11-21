using FluentAssertions;

namespace CO.CDP.OrganisationApp.Tests;

public class UserInfoTest
{
    [Fact]
    public void ItChecksIfUserIsAssignedToAnyOrganisations()
    {
        new UserInfo
        {
            Email = "contact@example.com",
            Name = "Alice",
            Organisations =
            [
                new UserOrganisationInfo
                {
                    Id = Guid.NewGuid(),
                    Name = "Acme",
                    Roles = [],
                    PendingRoles = [],
                    Scopes = []
                }
            ],
            Scopes = []
        }.HasOrganisations().Should().Be(true);

        new UserInfo
        {
            Email = "contact@example.com",
            Name = "Alice",
            Organisations = [],
            Scopes = []
        }.HasOrganisations().Should().Be(false);
    }

    [Fact]
    public void ItReturnsScopesOfTheOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var unkownOrganisationId = Guid.NewGuid();
        var userInfo = new UserInfo
        {
            Email = "contact@example.com",
            Name = "Alice",
            Organisations =
            [
                new UserOrganisationInfo
                {
                    Id = organisationId,
                    Name = "Acme",
                    Roles = [],
                    PendingRoles = [],
                    Scopes = ["ADMIN", "VIEWER"]
                }
            ],
            Scopes = ["SUPPORT"]
        };

        userInfo.OrganisationScopes(organisationId).Should().BeEquivalentTo(["ADMIN", "VIEWER"]);
        userInfo.OrganisationScopes(unkownOrganisationId).Should().BeEquivalentTo([]);
        userInfo.OrganisationScopes(null).Should().BeEquivalentTo([]);
    }
}