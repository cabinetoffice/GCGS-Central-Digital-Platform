using CO.CDP.UserManagement.App.Mapping;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;

namespace CO.CDP.UserManagement.App.Tests.Mapping;

public class ViewModelMapperTests
{
    [Fact]
    public void ToHomeViewModel_CalculatesStats()
    {
        var org = BuildOrganisationResponse();
        var enabledApps = new List<OrganisationApplicationResponse>
        {
            BuildOrganisationApplicationResponse(org.Id, 10, true),
            BuildOrganisationApplicationResponse(org.Id, 11, false)
        };
        var roles = new List<RoleResponse> { BuildRoleResponse(10), BuildRoleResponse(11) };
        var users = new List<OrganisationUserResponse>
        {
            BuildOrganisationUserResponse(org.Id, UserStatus.Active),
            BuildOrganisationUserResponse(org.Id, UserStatus.Pending)
        };

        var result = ViewModelMapper.ToHomeViewModel(org, enabledApps, roles, users);

        result.Stats.ApplicationsEnabled.Should().Be(1);
        result.Stats.TotalUsers.Should().Be(1);
        result.Stats.RolesAssigned.Should().Be(2);
    }

    [Fact]
    public void ToApplicationsViewModel_FiltersByStatus()
    {
        var org = BuildOrganisationResponse();
        var allApps = new List<ApplicationResponse>
        {
            BuildApplicationResponse(10, "app-1", "CatA"),
            BuildApplicationResponse(11, "app-2", "CatB")
        };
        var enabledApps = new List<OrganisationApplicationResponse>
        {
            BuildOrganisationApplicationResponse(org.Id, 10, true)
        };

        var result = ViewModelMapper.ToApplicationsViewModel(
            org,
            allApps,
            enabledApps,
            selectedStatus: "enabled");

        result.EnabledApplications.Should().HaveCount(1);
        result.AvailableApplications.Should().BeEmpty();
        result.Categories.Should().BeEquivalentTo(new[] { "CatA", "CatB" });
    }

    [Fact]
    public void ToApplicationsViewModel_FiltersBySearch()
    {
        var org = BuildOrganisationResponse();
        var allApps = new List<ApplicationResponse>
        {
            BuildApplicationResponse(10, "app-1", "CatA", "Search Me"),
            BuildApplicationResponse(11, "app-2", "CatB", "Other")
        };
        var enabledApps = new List<OrganisationApplicationResponse>();

        var result = ViewModelMapper.ToApplicationsViewModel(
            org,
            allApps,
            enabledApps,
            searchTerm: "search");

        result.AvailableApplications.Should().ContainSingle(app => app.Name == "Search Me");
    }

    [Fact]
    public void ToApplicationDetailsViewModel_CalculatesPermissions()
    {
        var org = BuildOrganisationResponse();
        var orgApp = BuildOrganisationApplicationResponse(org.Id, 10, true);
        var roles = new List<RoleResponse>
        {
            BuildRoleResponse(10, new List<PermissionResponse>
            {
                BuildPermissionResponse(1, 10),
                BuildPermissionResponse(2, 10)
            })
        };
        var assignments = new List<UserAssignmentResponse>
        {
            BuildUserAssignmentResponse(1, isActive: true),
            BuildUserAssignmentResponse(2, isActive: false)
        };

        var result = ViewModelMapper.ToApplicationDetailsViewModel(org, orgApp, roles, assignments);

        result.UsersAssigned.Should().Be(1);
        result.TotalPermissions.Should().Be(2);
    }

    [Fact]
    public void ToDisableApplicationViewModel_UsesActiveAssignments()
    {
        var org = BuildOrganisationResponse();
        var orgApp = BuildOrganisationApplicationResponse(org.Id, 10, true);
        var assignments = new List<UserAssignmentResponse>
        {
            BuildUserAssignmentResponse(1, isActive: true),
            BuildUserAssignmentResponse(2, isActive: false)
        };

        var result = ViewModelMapper.ToDisableApplicationViewModel(org, orgApp, assignments);

        result.ActiveAssignments.Should().Be(1);
    }

    private static OrganisationResponse BuildOrganisationResponse() =>
        new()
        {
            Id = 1,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org",
            Slug = "org",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private static OrganisationApplicationResponse BuildOrganisationApplicationResponse(int orgId, int appId, bool isActive) =>
        new()
        {
            Id = 1,
            OrganisationId = orgId,
            ApplicationId = appId,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            Application = BuildApplicationResponse(appId, $"app-{appId}", "CatA")
        };

    private static ApplicationResponse BuildApplicationResponse(int id, string clientId, string category, string? name = null) =>
        new()
        {
            Id = id,
            Name = name ?? $"App {id}",
            ClientId = clientId,
            Description = "desc",
            Category = category,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private static RoleResponse BuildRoleResponse(int appId, List<PermissionResponse>? permissions = null) =>
        new()
        {
            Id = 1,
            ApplicationId = appId,
            Name = "Role",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            Permissions = permissions
        };

    private static PermissionResponse BuildPermissionResponse(int id, int appId) =>
        new()
        {
            Id = id,
            ApplicationId = appId,
            Name = $"Perm {id}",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system"
        };

    private static OrganisationUserResponse BuildOrganisationUserResponse(int orgId, UserStatus status) =>
        new()
        {
            MembershipId = 1,
            OrganisationId = orgId,
            CdpPersonId = Guid.NewGuid(),
            FirstName = "User",
            LastName = "Test",
            Email = "user@example.com",
            OrganisationRole = OrganisationRole.Member,
            Status = status,
            IsActive = status == UserStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationAssignments = []
        };

    private static UserAssignmentResponse BuildUserAssignmentResponse(int id, bool isActive) =>
        new()
        {
            Id = id,
            UserOrganisationMembershipId = 1,
            OrganisationApplicationId = 1,
            IsActive = isActive,
            AssignedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
}
