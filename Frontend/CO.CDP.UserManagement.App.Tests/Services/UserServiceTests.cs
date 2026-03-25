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
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(),
                null));

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
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(),
                null));

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
        _apiClient.Setup(client =>
                client.UsersGET2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
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
        _apiClient.Setup(client =>
                client.UsersGET2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
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
        _apiClient.Setup(client =>
                client.UsersGET2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(),
                null));

        var result = await _service.GetUserDetailsViewModelAsync("org", personId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task
        GetUserDetailsViewModelAsync_WhenApplicationAssignmentsPresent_ReturnsViewModelWithApplicationAccess()
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
        _apiClient.Setup(client =>
                client.UsersGET2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
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
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, InviteUserRequest, CancellationToken>((_, request, _) => capturedRequest = request)
            .ReturnsAsync(BuildPendingInviteResponse(org.Id, 1, OrganisationRole.Member));

        var result = await _service.InviteUserAsync("org", new InviteUserViewModel
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                OrganisationRole = OrganisationRole.Member
            }, CancellationToken.None,
            [new InviteApplicationAssignment { OrganisationApplicationId = 10, ApplicationRoleId = 99 }]);

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
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.InviteUserAsync("org", InviteUserViewModel.Empty, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task InviteUserAsync_WhenApiExceptionIsServerError_ReturnsFalseAndSetsErrorFlag()
    {
        var service = new UserService(_apiClient.Object);
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty,
                new Dictionary<string, IEnumerable<string>>(), null));

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

        var result =
            await _service.GetChangeUserRoleViewModelAsync("org", user.CdpPersonId, null, CancellationToken.None);

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
        _apiClient.Setup(client => client.RoleAsync(org.CdpOrganisationGuid, 2,
                It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result =
            await _service.UpdateUserRoleAsync("org", null, inviteGuid, OrganisationRole.Admin, CancellationToken.None);

        result.IsRight().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenUser_ReturnsTrue()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.Role2Async(org.CdpOrganisationGuid, personId,
                It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result =
            await _service.UpdateUserRoleAsync("org", personId, null, OrganisationRole.Admin, CancellationToken.None);

        result.IsRight().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WhenNoTarget_ReturnsNotFoundOutcome()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);

        var result =
            await _service.UpdateUserRoleAsync("org", null, null, OrganisationRole.Admin, CancellationToken.None);

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
        _apiClient.Setup(client => client.Role2Async(org.CdpOrganisationGuid, personId,
                It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result =
            await _service.UpdateUserRoleAsync("org", personId, null, OrganisationRole.Admin, CancellationToken.None);

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
        _apiClient.Setup(client => client.Role2Async(org.CdpOrganisationGuid, personId,
                It.IsAny<ChangeOrganisationRoleRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty,
                new Dictionary<string, IEnumerable<string>>(), null));

        var result =
            await service.UpdateUserRoleAsync("org", personId, null, OrganisationRole.Admin, CancellationToken.None);

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
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _apiClient.Setup(client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

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
        _apiClient.Setup(client => client.InvitesPOSTAsync(org.CdpOrganisationGuid, It.IsAny<InviteUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _apiClient.Setup(client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty,
                new Dictionary<string, IEnumerable<string>>(), null));

        var result = await service.ResendInviteAsync("org", inviteGuid, CancellationToken.None);

        result.IsLeft().Should().BeTrue();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenOrganisationMissing_ReturnsNull()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(),
                null));

        var result = await _service.GetRemoveUserViewModelAsync("org", null, null, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenPendingInviteMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse>());

        var result = await _service.GetRemoveUserViewModelAsync("org", null, 99, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenPendingInviteFound_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var invite = BuildPendingInviteResponse(org.Id, 2, OrganisationRole.Admin);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse> { invite });

        var result = await _service.GetRemoveUserViewModelAsync("org", null, 2, CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationName.Should().Be("Org");
        result.OrganisationSlug.Should().Be("org");
        result.UserDisplayName.Should().Be("Test User");
        result.Email.Should().Be("test@example.com");
        result.CurrentRole.Should().Be(OrganisationRole.Admin);
        result.MemberSinceFormatted.Should().Match("* * *");
        result.PendingInviteId.Should().Be(2);
        result.CdpPersonId.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenPendingInviteFoundWithoutName_ReturnsViewModelWithEmptyName()
    {
        var org = BuildOrganisationResponse();
        var invite = new PendingOrganisationInviteResponse
        {
            CdpPersonInviteGuid = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Email = "test@example.com",
            ExpiresOn = null,
            FirstName = null,
            InvitedBy = "inviter",
            LastName = null,
            OrganisationId = org.Id,
            OrganisationRole = OrganisationRole.Member,
            PendingInviteId = 3,
            Status = UserStatus.Pending
        };
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesAllAsync(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PendingOrganisationInviteResponse> { invite });

        var result = await _service.GetRemoveUserViewModelAsync("org", null, 3, CancellationToken.None);

        result.Should().NotBeNull();
        result!.UserDisplayName.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenUserMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client =>
                client.UsersGET2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganisationUserResponse?)null);

        var result = await _service.GetRemoveUserViewModelAsync("org", personId, null, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenUserFound_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        var user = new OrganisationUserResponse
        {
            MembershipId = 1,
            OrganisationId = org.Id,
            CdpPersonId = personId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            OrganisationRole = OrganisationRole.Owner,
            Status = UserStatus.Active,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero),
            ApplicationAssignments = []
        };
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client =>
                client.UsersGET2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.GetRemoveUserViewModelAsync("org", personId, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationName.Should().Be("Org");
        result.OrganisationSlug.Should().Be("org");
        result.UserDisplayName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.CurrentRole.Should().Be(OrganisationRole.Owner);
        result.MemberSinceFormatted.Should().Be("15 January 2025");
        result.CdpPersonId.Should().Be(personId);
        result.PendingInviteId.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenUserFoundWithoutName_ReturnsViewModelWithEmptyName()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        var user = new OrganisationUserResponse
        {
            MembershipId = 1,
            OrganisationId = org.Id,
            CdpPersonId = personId,
            FirstName = null,
            LastName = null,
            Email = "test@example.com",
            OrganisationRole = OrganisationRole.Member,
            Status = UserStatus.Active,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationAssignments = []
        };
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client =>
                client.UsersGET2Async(org.CdpOrganisationGuid, personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _service.GetRemoveUserViewModelAsync("org", personId, null, CancellationToken.None);

        result.Should().NotBeNull();
        result!.UserDisplayName.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRemoveUserViewModelAsync_WhenApiException_ReturnsNull()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(),
                null));

        var result = await _service.GetRemoveUserViewModelAsync("org", null, null, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveUserAsync_WhenOrganisationMissing_ReturnsFalse()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(),
                null));

        var result = await _service.RemoveUserAsync("org", null, null, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveUserAsync_WhenPendingInviteDeleted_ReturnsTrue()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 5, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.RemoveUserAsync("org", null, 5, CancellationToken.None);

        result.Should().BeTrue();
        _apiClient.Verify(
            client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUserAsync_WhenPendingInviteDeleteFails_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.InvitesDELETEAsync(org.CdpOrganisationGuid, 5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.RemoveUserAsync("org", null, 5, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveUserAsync_WhenCdpPersonIdProvided_ReturnsTrue()
    {
        var org = BuildOrganisationResponse();
        var personId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);

        var result = await _service.RemoveUserAsync("org", personId, null, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUserAsync_WhenNeitherIdProvided_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);

        var result = await _service.RemoveUserAsync("org", null, null, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveUserAsync_WhenApiExceptionOnOrgLookup_ReturnsFalse()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new ApiException("Bad", 500, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.RemoveUserAsync("org", null, 5, CancellationToken.None);

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
        _apiClient.Setup(client =>
                client.RolesAll2Async(org.Id, 20, state.OrganisationRole, It.IsAny<CancellationToken>()))
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
        _apiClient.Setup(client =>
                client.RolesAll2Async(org.Id, It.IsAny<int>(), state.OrganisationRole, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty,
                new Dictionary<string, IEnumerable<string>>(), null));
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
            .ThrowsAsync(new ApiException("Server error", 500, string.Empty,
                new Dictionary<string, IEnumerable<string>>(), null));

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

    [Fact]
    public async Task GetRemoveApplicationSuccessViewModelAsync_WhenOrganisationNotFound_ReturnsNull()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(),
                null));

        var result =
            await _service.GetRemoveApplicationSuccessViewModelAsync("org", Guid.NewGuid(), "test-app",
                CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveApplicationSuccessViewModelAsync_WhenUserNotFound_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        var userId = Guid.NewGuid();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.UsersAll2Async(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationUserResponse>());

        var result =
            await _service.GetRemoveApplicationSuccessViewModelAsync("org", userId, "test-app", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveApplicationSuccessViewModelAsync_WhenApplicationNotFound_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        var userId = Guid.NewGuid();
        var users = new List<OrganisationUserResponse>
        {
            new()
            {
                MembershipId = 1,
                OrganisationId = org.Id,
                CdpPersonId = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
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
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationResponse>());

        var result =
            await _service.GetRemoveApplicationSuccessViewModelAsync("org", userId, "non-existent-app",
                CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRemoveApplicationSuccessViewModelAsync_WhenValid_ReturnsPopulatedViewModel()
    {
        var org = BuildOrganisationResponse();
        var userId = Guid.NewGuid();
        var appClientId = "test-app";
        var users = new List<OrganisationUserResponse>
        {
            new()
            {
                MembershipId = 1,
                OrganisationId = org.Id,
                CdpPersonId = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                OrganisationRole = OrganisationRole.Admin,
                Status = UserStatus.Active,
                IsActive = true,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ApplicationAssignments = []
            }
        };
        var apps = new List<ApplicationResponse>
        {
            new()
            {
                Id = 42,
                Name = "Test Application",
                ClientId = appClientId,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.UsersAll2Async(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apps);

        var result =
            await _service.GetRemoveApplicationSuccessViewModelAsync("org", userId, appClientId,
                CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationSlug.Should().Be("org");
        result.UserDisplayName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.ApplicationName.Should().Be("Test Application");
        result.CdpPersonId.Should().Be(userId);
    }
}