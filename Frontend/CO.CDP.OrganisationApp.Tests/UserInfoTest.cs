using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
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

    [Theory]
    [MemberData(nameof(ViewerScopeCases))]
    public void ItChecksIfUserHasViewerScope(
        List<string> userScopes,
        List<string> userOrganisationScopes,
        Guid actualOrganisationId,
        Guid checkedOrganisationId,
        bool outcome)
    {
        var testedOrganisationInfo = new UserOrganisationInfo
        {
            Id = actualOrganisationId,
            Name = "Acme",
            Roles = [PartyRole.Supplier],
            PendingRoles = [],
            Scopes = userOrganisationScopes
        };
        var notTestedOrganisationInfo = new UserOrganisationInfo
        {
            Id = Guid.NewGuid(),
            Name = "Test Labs Ltd",
            Roles = [PartyRole.Supplier],
            PendingRoles = [],
            Scopes = [OrganisationPersonScopes.Admin]
        };
        var userInfo = new UserInfo
        {
            Email = "contact@example.com",
            Name = "Alice",
            Organisations = [testedOrganisationInfo, notTestedOrganisationInfo],
            Scopes = userScopes
        };

        userInfo.IsViewerOf(checkedOrganisationId).Should().Be(outcome);
    }

    public static IEnumerable<object[]> ViewerScopeCases() => new List<object[]>
    {
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Viewer },
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            true
        },
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Viewer },
            Guid.Parse("5c08b265-8cad-466d-a8d3-c7096c51e2b1"),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            false
        },
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Admin },
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            false
        },
        new object[]
        {
            new List<string>(),
            new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Viewer },
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            true
        },
        new object[]
        {
            new List<string> { PersonScopes.SupportAdmin },
            new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Viewer },
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            true
        },
        new object[]
        {
            new List<string> { PersonScopes.SupportAdmin },
            new List<string> { OrganisationPersonScopes.Admin },
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            false
        },
        new object[]
        {
            new List<string> { PersonScopes.SupportAdmin },
            new List<string>(),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            Guid.Parse("49c8b8de-457a-40ee-9c29-bb1aa900941c"),
            true
        }
    };
}