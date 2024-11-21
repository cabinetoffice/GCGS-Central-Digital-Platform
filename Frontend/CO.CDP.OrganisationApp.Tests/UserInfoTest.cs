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
}