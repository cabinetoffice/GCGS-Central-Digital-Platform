using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.WebApiClient;
using CO.CDP.Functional;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<UserManagementClient> _apiClient;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _apiClient = new Mock<UserManagementClient>("http://localhost", new HttpClient());
        _service = new UserService(_apiClient.Object);
    }

    [Fact]
    public async Task GetUsersViewModelAsync_WhenOrganisationMissing_ReturnsNull()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.GetUsersViewModelAsync("org", ct: CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersViewModelAsync_WhenFilteredByRole_ReturnsFilteredUsers()
    {
        var org = BuildOrganisationResponse();
        var users = new List<OrganisationUserResponse>
        {
            new()
            {
                MembershipId = 1,
                OrganisationId = org.Id,
                CdpPersonId = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                OrganisationRole = OrganisationRole.Admin,
                Status = UserStatus.Active,
                IsActive = true,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ApplicationAssignments = []
            }
        };
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.UsersAll2Async(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse>());

        var result = await _service.GetUsersViewModelAsync("org", selectedRole: "Admin", ct: CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetInviteUserViewModelAsync_WhenOrganisationMissing_ReturnsNull()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.GetInviteUserViewModelAsync("org", ct: CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetInviteUserViewModelAsync_WhenValid_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);

        var result = await _service.GetInviteUserViewModelAsync("org", ct: CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationName.Should().Be("Org");
    }

    [Fact]
    public async Task InviteUserAsync_WhenSuccess_ReturnsTrue()
    {
        var org = BuildOrganisationResponse();
        InviteUserRequest? capturedRequest = null;
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, InviteUserRequest, CancellationToken>((_, request, _) => capturedRequest = request)
            .ReturnsAsync(BuildPendingInviteResponse(org.Id, 1, OrganisationRole.Member));

        var result = await _service.InviteUserAsync("org", new Models.InviteUserViewModel
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            OrganisationRole = OrganisationRole.Member
        }, CancellationToken.None, [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 99 }]);

        result.IsRight().Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.ApplicationAssignments.Should().HaveCount(1);
        var assignment = capturedRequest.ApplicationAssignments!.Single();
        assignment.OrganisationApplicationId.Should().Be(10);
        assignment.ApplicationRoleIds.Should().ContainSingle().Which.Should().Be(99);
    }

    [Fact]
    public async Task InviteUserAsync_WhenApiException_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.InviteUserAsync("org", Models.InviteUserViewModel.Empty, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task InviteUserAsync_WhenApiExceptionIsServerError_ReturnsFalseAndSetsErrorFlag()
    {
                var service = new UserService(_apiClient.Object);
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await service.InviteUserAsync("org", Models.InviteUserViewModel.Empty, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task GetChangeUserRoleViewModelAsync_WhenPendingInviteMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        var inviteGuid = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse>());

        var result = await _service.GetChangeUserRoleViewModelAsync("org", null, inviteGuid, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetChangeUserRoleViewModelAsync_WhenPendingInviteFound_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var inviteGuid = Guid.NewGuid();
        var invite = BuildPendingInviteResponse(org.Id, 2, OrganisationRole.Admin, inviteGuid);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse> { invite });

        var result = await _service.GetChangeUserRoleViewModelAsync("org", null, inviteGuid, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsPending.Should().BeTrue();
    }

    [Fact]
    public async Task GetChangeUserRoleViewModelAsync_WhenUserFound_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var user = new OrganisationUserResponse
        {
            MembershipId = 1,
            OrganisationId = org.Id,
            CdpPersonId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            OrganisationRole = OrganisationRole.Admin,
            Status = UserStatus.Active,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationAssignments = []
        };
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.UsersAll2Async(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationUserResponse> { user });

        var result = await _service.GetChangeUserRoleViewModelAsync("org", user.CdpPersonId, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsPending.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenPendingInvite_ReturnsTrue()
    {
        var org = BuildOrganisationResponse();
        var inviteGuid = Guid.NewGuid();
        var invite = BuildPendingInviteResponse(org.Id, 2, OrganisationRole.Admin, inviteGuid);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse> { invite });
        _apiClient.Setup(client => client.RoleAsync(org.CdpOrganisationGuid, 2, It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateUserRoleAsync("org", null, inviteGuid, OrganisationRole.Admin, CancellationToken.None);

        result.IsRight().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenUser_ReturnsTrue()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Role2Async(org.CdpOrganisationGuid, personId, It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateUserRoleAsync("org", personId, null, OrganisationRole.Admin, CancellationToken.None);

        result.IsRight().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenNoTarget_ReturnsNotFoundOutcome()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);

        var result = await _service.UpdateUserRoleAsync("org", null, null, OrganisationRole.Admin, CancellationToken.None);

        result.Match(
            _ => throw new Exception("Expected right-side not-found outcome."),
            outcome => outcome.Should().Be(ServiceOutcome.NotFound));
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenApiException_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Role2Async(org.CdpOrganisationGuid, personId, It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.UpdateUserRoleAsync("org", personId, null, OrganisationRole.Admin, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenApiExceptionIsServerError_ReturnsFalseAndSetsErrorFlag()
    {
                var service = new UserService(_apiClient.Object);
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Role2Async(org.CdpOrganisationGuid, personId, It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await service.UpdateUserRoleAsync("org", personId, null, OrganisationRole.Admin, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task ResendInviteAsync_WhenApiException_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        var inviteGuid = Guid.NewGuid();
        var invite = BuildPendingInviteResponse(org.Id, 1, OrganisationRole.Member, inviteGuid);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse> { invite });
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _apiClient.Setup(client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.ResendInviteAsync("org", inviteGuid, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task ResendInviteAsync_WhenApiExceptionIsServerError_ReturnsFalseAndSetsErrorFlag()
    {
                var service = new UserService(_apiClient.Object);
        var org = BuildOrganisationResponse();
        var inviteGuid = Guid.NewGuid();
        var invite = BuildPendingInviteResponse(org.Id, 1, OrganisationRole.Member, inviteGuid);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse> { invite });
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _apiClient.Setup(client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await service.ResendInviteAsync("org", inviteGuid, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task GetApplicationRolesStepViewModelAsync_WhenValid_ReturnsMappedApplications()
    {
        var org = BuildOrganisationResponse();
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        var organisationApps = new List<OrganisationApplicationResponse>
        {
            new()
            {
                Id = 10,
                OrganisationId = org.Id,
                ApplicationId = 20,
                IsActive = true,
                Application = new ApplicationResponse
                {
                    Id = 20,
                    Name = "Payments",
                    ClientId = "payments",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "system"
            }
        };
        var roles = new List<RoleResponse>
        {
            new()
            {
                Id = 1,
                ApplicationId = 20,
                Name = "Admin",
                Description = "Full access",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "system"
            }
        };

        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organisationApps);
        _apiClient.Setup(client => client.RolesAll2Async(org.Id, 20, state.OrganisationRole, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var result = await _service.GetApplicationRolesStepViewModelAsync("org", state, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Applications.Should().ContainSingle();
        result.Applications[0].ApplicationName.Should().Be("Payments");
        result.Applications[0].Roles.Should().ContainSingle(r => r.Name == "Admin");
    }

    [Fact]
    public async Task GetApplicationRolesStepViewModelAsync_WhenScopedRolesApiServerError_Throws()
    {
        var service = new UserService(_apiClient.Object);
        var org = BuildOrganisationResponse();
        var state = new InviteUserState("org", "user@example.com", "First", "Last");
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.RolesAll2Async(org.Id, It.IsAny<int>(), state.OrganisationRole, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationApplicationResponse>
            {
                new()
                {
                    Id = 1,
                    OrganisationId = org.Id,
                    ApplicationId = 20,
                    IsActive = true,
                    Application = new ApplicationResponse
                    {
                        Id = 20,
                        Name = "Payments",
                        ClientId = "payments",
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    },
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "system"
                }
            });

        await Assert.ThrowsAsync<ApiException>(() =>
            service.GetApplicationRolesStepViewModelAsync("org", state, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateUserApplicationRolesAsync_WhenApiExceptionIsServerError_ReturnsFalseAndSetsErrorFlag()
    {
                var service = new UserService(_apiClient.Object);
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.UsersAll2Async(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await service.UpdateUserApplicationRolesAsync(
            "org",
            personId,
            null,
            new List<ApplicationRoleAssignmentPostModel>(),
            CancellationToken.None);

        result.IsLeft().Should().BeTrue();
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

    private static PendingOrganisationInviteResponse BuildPendingInviteResponse(
        int organisationId,
        int pendingInviteId,
        OrganisationRole organisationRole,
        Guid? cdpPersonInviteGuid = null)
    {
        return new PendingOrganisationInviteResponse
        {
            CdpPersonInviteGuid = cdpPersonInviteGuid ?? Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Email = "test@example.com",
            ExpiresOn = null,
            FirstName = "Test",
            InvitedBy = "inviter",
            LastName = "User",
            OrganisationId = organisationId,
            OrganisationRole = organisationRole,
            PendingInviteId = pendingInviteId,
            Status = UserStatus.Pending
        };
    }
}
