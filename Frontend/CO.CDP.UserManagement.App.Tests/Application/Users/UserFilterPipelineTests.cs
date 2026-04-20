using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;

namespace CO.CDP.UserManagement.App.Tests.Application.Users;

public class UserFilterPipelineTests : AdapterTestFixture
{
    // ── UsersFilter ───────────────────────────────────────────────────────────

    [Fact]
    public void UsersFilter_IsActive_FalseWhenAllNull()
    {
        var filter = new UsersFilter(null, null, null);
        filter.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("admin", null, null)]
    [InlineData(null, "app-1", null)]
    [InlineData(null, null, "alice")]
    [InlineData("admin", "app-1", "alice")]
    public void UsersFilter_IsActive_TrueWhenAnyFilterSet(string? role, string? app, string? search)
    {
        var filter = new UsersFilter(role, app, search);
        filter.IsActive.Should().BeTrue();
    }

    // ── MatchesRole ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Admin", null, true)]
    [InlineData("Admin", "admin", true)]
    [InlineData("Admin", "ADMIN", true)]
    [InlineData("Admin", "member", false)]
    [InlineData("Owner", "admin", false)]
    public void MatchesRole_ReturnsExpected(string roleEnumName, string? filter, bool expected)
    {
        UserFilterPipeline.MatchesRole(roleEnumName, filter).Should().Be(expected);
    }

    // ── MatchesSearch ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Alice Smith", "alice@example.com", null, true)]
    [InlineData("Alice Smith", "alice@example.com", "alice", true)]
    [InlineData("Alice Smith", "alice@example.com", "ALICE", true)]
    [InlineData("Alice Smith", "alice@example.com", "smith", true)]
    [InlineData("Alice Smith", "alice@example.com", "alice@", true)]
    [InlineData("Bob Jones", "bob@example.com", "alice", false)]
    public void MatchesSearch_ReturnsExpected(string name, string email, string? search, bool expected)
    {
        UserFilterPipeline.MatchesSearch(name, email, search).Should().Be(expected);
    }

    // ── UserMatchesApplication ────────────────────────────────────────────────

    [Fact]
    public void UserMatchesApplication_NullFilter_ReturnsTrue()
    {
        UserFilterPipeline.UserMatchesApplication(null, null).Should().BeTrue();
    }

    [Fact]
    public void UserMatchesApplication_MatchesByOrgApplicationId()
    {
        var assignments = new[]
        {
            new UserAssignmentResponse { Id = 1, UserOrganisationMembershipId = 1, OrganisationApplicationId = 42, IsActive = true, CreatedAt = DateTimeOffset.UtcNow }
        };
        UserFilterPipeline.UserMatchesApplication(assignments, "42").Should().BeTrue();
    }

    [Fact]
    public void UserMatchesApplication_MatchesByClientId()
    {
        var assignments = new[]
        {
            new UserAssignmentResponse
            {
                Id = 1, UserOrganisationMembershipId = 1, OrganisationApplicationId = 42, IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                Application = new ApplicationResponse
                {
                    Id = 5,
                    ClientId = "my-app",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Name = "name"
                }
            }
        };
        UserFilterPipeline.UserMatchesApplication(assignments, "my-app").Should().BeTrue();
    }

    [Fact]
    public void UserMatchesApplication_NoMatch_ReturnsFalse()
    {
        var assignments = new[]
        {
            new UserAssignmentResponse { Id = 1, UserOrganisationMembershipId = 1, OrganisationApplicationId = 10, IsActive = true, CreatedAt = DateTimeOffset.UtcNow }
        };
        UserFilterPipeline.UserMatchesApplication(assignments, "99").Should().BeFalse();
    }

    // ── InviteMatchesApplication ──────────────────────────────────────────────

    [Fact]
    public void InviteMatchesApplication_NullFilter_ReturnsTrue()
    {
        UserFilterPipeline.InviteMatchesApplication(null, null, new List<OrganisationApplicationResponse>()).Should().BeTrue();
    }

    [Fact]
    public void InviteMatchesApplication_MatchesByOrgApplicationId()
    {
        var assignments = new[]
        {
            new InviteApplicationAssignmentResponse { OrganisationApplicationId = 42, ApplicationRoleId = 1 }
        };
        UserFilterPipeline.InviteMatchesApplication(assignments, "42", new List<OrganisationApplicationResponse>()).Should().BeTrue();
    }

    [Fact]
    public void InviteMatchesApplication_MatchesByClientIdViaAppLookup()
    {
        var assignments = new[]
        {
            new InviteApplicationAssignmentResponse { OrganisationApplicationId = 99, ApplicationRoleId = 1 }
        };
        var apps = new[] { MakeApplication(orgAppId: 99, appId: 5, name: "Test") };
        UserFilterPipeline.InviteMatchesApplication(assignments, "app-5", apps).Should().BeTrue();
    }

    [Fact]
    public void InviteMatchesApplication_NoMatch_ReturnsFalse()
    {
        var assignments = new[]
        {
            new InviteApplicationAssignmentResponse { OrganisationApplicationId = 10, ApplicationRoleId = 1 }
        };
        UserFilterPipeline.InviteMatchesApplication(assignments, "99", new List<OrganisationApplicationResponse>()).Should().BeFalse();
    }

    // ── ApplyTo (users) ───────────────────────────────────────────────────────

    [Fact]
    public void ApplyTo_Users_RoleFilter_ExcludesNonMatchingRole()
    {
        var users = new[]
        {
            MakeUser(role: OrganisationRole.Admin),
            MakeUser(role: OrganisationRole.Member)
        };
        var result = UserFilterPipeline.ApplyTo(users, new UsersFilter("admin", null, null));
        result.Should().HaveCount(1);
        result[0].OrganisationRole.Should().Be(OrganisationRole.Admin);
    }

    [Fact]
    public void ApplyTo_Users_SearchFilter_ExcludesNonMatchingName()
    {
        var users = new[]
        {
            MakeUser(firstName: "Alice", lastName: "Smith"),
            MakeUser(firstName: "Bob", lastName: "Jones")
        };
        var result = UserFilterPipeline.ApplyTo(users, new UsersFilter(null, null, "alice"));
        result.Should().HaveCount(1);
        result[0].FirstName.Should().Be("Alice");
    }

    [Fact]
    public void ApplyTo_Users_CombinedFilters_AllMustMatch()
    {
        var users = new[]
        {
            MakeUser(firstName: "Alice", lastName: "Smith", role: OrganisationRole.Admin),
            MakeUser(firstName: "Alice", lastName: "Brown", role: OrganisationRole.Member),
            MakeUser(firstName: "Bob", lastName: "Jones", role: OrganisationRole.Admin),
        };
        var result = UserFilterPipeline.ApplyTo(users, new UsersFilter("admin", null, "alice"));
        result.Should().HaveCount(1);
        result[0].FirstName.Should().Be("Alice");
        result[0].OrganisationRole.Should().Be(OrganisationRole.Admin);
    }

    // ── ApplyTo (invites) ─────────────────────────────────────────────────────

    [Fact]
    public void ApplyTo_Invites_RoleFilter_ExcludesNonMatchingRole()
    {
        var invites = new[]
        {
            MakeInvite(role: OrganisationRole.Owner),
            MakeInvite(role: OrganisationRole.Member)
        };
        var result = UserFilterPipeline.ApplyTo(invites, new UsersFilter("owner", null, null), new List<OrganisationApplicationResponse>());
        result.Should().HaveCount(1);
        result[0].OrganisationRole.Should().Be(OrganisationRole.Owner);
    }

    [Fact]
    public void ApplyTo_Invites_SearchFilter_ExcludesNonMatchingEmail()
    {
        var invites = new[]
        {
            MakeInvite(email: "alice@example.com", firstName: "Alice", lastName: "Smith"),
            MakeInvite(email: "bob@example.com", firstName: "Bob", lastName: "Jones")
        };
        var result = UserFilterPipeline.ApplyTo(invites, new UsersFilter(null, null, "alice"), new List<OrganisationApplicationResponse>());
        result.Should().HaveCount(1);
        result[0].Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void ApplyTo_Invites_ApplicationFilter_ExcludesNonMatchingApp()
    {
        var app = MakeApplication(orgAppId: 7, appId: 3, name: "MyApp");
        var inviteWithApp = MakeInvite() with
        {
            ApplicationAssignments = new[]
            {
                new InviteApplicationAssignmentResponse { OrganisationApplicationId = 7, ApplicationRoleId = 1 }
            }
        };
        var inviteWithoutApp = MakeInvite(inviteGuid: Guid.NewGuid(), pendingInviteId: 2);

        var result = UserFilterPipeline.ApplyTo(
            new[] { inviteWithApp, inviteWithoutApp },
            new UsersFilter(null, "app-3", null),
            new[] { app });

        result.Should().HaveCount(1);
    }

    [Fact]
    public void ApplyTo_Invites_NoFilter_ReturnsAll()
    {
        var invites = new[] { MakeInvite(), MakeInvite(inviteGuid: Guid.NewGuid(), pendingInviteId: 2) };
        var result = UserFilterPipeline.ApplyTo(invites, UsersFilter.None, new List<OrganisationApplicationResponse>());
        result.Should().HaveCount(2);
    }
}
