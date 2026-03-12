using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.WebApiClient;
using FluentAssertions;
using Moq;
using ApiClient = CO.CDP.UserManagement.WebApiClient;

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
            .ReturnsAsync(new List<ApiClient.PendingOrganisationInviteResponse>());

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
    public async Task GetUserDetailsViewModelAsync_WhenValid_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        var joinedAt = new DateTimeOffset(2026, 2, 19, 0, 0, 0, TimeSpan.Zero);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Users2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationUserResponse
            {
                MembershipId = 1,
                OrganisationId = org.Id,
                CdpPersonId = personId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                OrganisationRole = OrganisationRole.Admin,
                Status = UserStatus.Active,
                IsActive = true,
                JoinedAt = joinedAt,
                CreatedAt = DateTimeOffset.UtcNow,
                ApplicationAssignments = []
            });

        var result = await _service.GetUserDetailsViewModelAsync("org", personId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.FullName.Should().Be("Test User");
        result.MemberSince.Should().Be("19 February 2026");
    }

    [Fact]
    public async Task GetUserDetailsViewModelAsync_WhenJoinedAtMissing_ReturnsNotAvailableMemberSince()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Users2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationUserResponse
            {
                MembershipId = 1,
                OrganisationId = org.Id,
                CdpPersonId = personId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                OrganisationRole = OrganisationRole.Admin,
                Status = UserStatus.Active,
                IsActive = true,
                JoinedAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                ApplicationAssignments = []
            });

        var result = await _service.GetUserDetailsViewModelAsync("org", personId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.MemberSince.Should().Be("Not available");
    }

    [Fact]
    public async Task GetUserDetailsViewModelAsync_WhenUserMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Users2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.GetUserDetailsViewModelAsync("org", personId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserDetailsViewModelAsync_WhenApplicationAssignmentsPresent_ReturnsViewModelWithApplicationAccess()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        var joinedAt = new DateTimeOffset(2026, 2, 19, 0, 0, 0, TimeSpan.Zero);
        var assignedDate = new DateTimeOffset(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

        var applicationAssignments = new List<UserAssignmentResponse>
        {
            new()
            {
                Id = 1,
                UserOrganisationMembershipId = 1,
                OrganisationApplicationId = 10,
                ApplicationId = 101,
                IsActive = true,
                AssignedAt = assignedDate,
                AssignedBy = "admin@example.com",
                CreatedAt = DateTimeOffset.UtcNow,
                Application = new ApplicationResponse
                {
                    Id = 101,
                    Name = "Edit",
                    Description = "Edit application",
                    ClientId = "edit-app",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                Roles = new List<RoleResponse>
                {
                    new()
                    {
                        Id = 1001,
                        ApplicationId = 101,
                        Name = "Admin",
                        Description = "Administrator role",
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "system",
                        Permissions = new List<PermissionResponse>
                        {
                            new()
                            {
                                Id = 2001,
                                ApplicationId = 101,
                                Name = "Read",
                                Description = "Read permission",
                                IsActive = true,
                                CreatedAt = DateTimeOffset.UtcNow,
                                CreatedBy = "system"
                            },
                            new()
                            {
                                Id = 2002,
                                ApplicationId = 101,
                                Name = "Write",
                                Description = "Write permission",
                                IsActive = true,
                                CreatedAt = DateTimeOffset.UtcNow,
                                CreatedBy = "system"
                            }
                        }
                    }
                }
            },
            new()
            {
                Id = 2,
                UserOrganisationMembershipId = 1,
                OrganisationApplicationId = 11,
                ApplicationId = 102,
                IsActive = true,
                AssignedAt = assignedDate.AddDays(-5),
                AssignedBy = "admin@example.com",
                CreatedAt = DateTimeOffset.UtcNow,
                Application = new ApplicationResponse
                {
                    Id = 102,
                    Name = "View",
                    Description = "View application",
                    ClientId = "view-app",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                Roles = new List<RoleResponse>
                {
                    new()
                    {
                        Id = 1002,
                        ApplicationId = 102,
                        Name = "Editor",
                        Description = "Editor role",
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "system",
                        Permissions = new List<PermissionResponse>
                        {
                            new()
                            {
                                Id = 2003,
                                ApplicationId = 102,
                                Name = "Read",
                                Description = "Read permission",
                                IsActive = true,
                                CreatedAt = DateTimeOffset.UtcNow,
                                CreatedBy = "system"
                            }
                        }
                    }
                }
            }
        };

        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Users2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationUserResponse
            {
                MembershipId = 1,
                OrganisationId = org.Id,
                CdpPersonId = personId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                OrganisationRole = OrganisationRole.Admin,
                Status = UserStatus.Active,
                IsActive = true,
                JoinedAt = joinedAt,
                CreatedAt = DateTimeOffset.UtcNow,
                ApplicationAssignments = applicationAssignments
            });

        var result = await _service.GetUserDetailsViewModelAsync("org", personId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ApplicationAccess.Should().HaveCount(2);
        result.ApplicationAccess[0].ApplicationName.Should().Be("Edit");
        result.ApplicationAccess[0].ApplicationRole.Should().Be("Admin");
        result.ApplicationAccess[0].Permissions.Should().ContainInOrder(["Read", "Write"]);
        result.ApplicationAccess[0].AssignedByEmail.Should().Be("admin@example.com");
        result.ApplicationAccess[1].ApplicationName.Should().Be("View");
        result.ApplicationAccess[1].ApplicationRole.Should().Be("Editor");
        result.ApplicationAccess[1].Permissions.Should().ContainInOrder(["Read"]);
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

        result.Should().BeTrue();
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

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetChangeUserRoleViewModelAsync_WhenPendingInviteMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        var inviteGuid = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApiClient.PendingOrganisationInviteResponse>());

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
            .ReturnsAsync(new List<ApiClient.PendingOrganisationInviteResponse> { invite });

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
            .ReturnsAsync(new List<ApiClient.PendingOrganisationInviteResponse> { invite });
        _apiClient.Setup(client => client.RoleAsync(org.CdpOrganisationGuid, 2, It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateUserRoleAsync("org", null, inviteGuid, OrganisationRole.Admin, CancellationToken.None);

        result.Should().BeTrue();
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

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenNoTarget_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);

        var result = await _service.UpdateUserRoleAsync("org", null, null, OrganisationRole.Admin, CancellationToken.None);

        result.Should().BeFalse();
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

        result.Should().BeFalse();
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
            .ReturnsAsync(new List<ApiClient.PendingOrganisationInviteResponse> { invite });
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _apiClient.Setup(client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.ResendInviteAsync("org", inviteGuid, CancellationToken.None);

        result.Should().BeFalse();
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
        _apiClient.Setup(client => client.RolesAllAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var result = await _service.GetApplicationRolesStepViewModelAsync("org", state, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Applications.Should().ContainSingle();
        result.Applications[0].ApplicationName.Should().Be("Payments");
        result.Applications[0].Roles.Should().ContainSingle(r => r.Name == "Admin");
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

    private static ApiClient.PendingOrganisationInviteResponse BuildPendingInviteResponse(
        int organisationId,
        int pendingInviteId,
        OrganisationRole organisationRole,
        Guid? cdpPersonInviteGuid = null)
    {
        return new ApiClient.PendingOrganisationInviteResponse(
            cdpPersonInviteGuid: cdpPersonInviteGuid ?? Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            email: "test@example.com",
            expiresOn: null,
            firstName: "Test",
            invitedBy: "inviter",
            lastName: "User",
            organisationId: organisationId,
            organisationRole: organisationRole,
            pendingInviteId: pendingInviteId,
            status: UserStatus.Pending);
    }
}
