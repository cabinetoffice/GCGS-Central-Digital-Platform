using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.WebApiClient;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Services;

public class ApplicationServiceTests
{
    private readonly Mock<UserManagementClient> _apiClient;
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _apiClient = new Mock<UserManagementClient>("http://localhost", new HttpClient());
        _service = new ApplicationService(_apiClient.Object);
    }

    [Fact]
    public async Task GetHomeViewModelAsync_WhenOrganisationMissing_ReturnsNull()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.GetHomeViewModelAsync("org", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetHomeViewModelAsync_WhenValid_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var app = BuildOrganisationApplicationResponse(org.Id, 10);
        var role = BuildRoleResponse(10);
        var user = BuildOrganisationUserResponse(org.Id);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationApplicationResponse> { app });
        _apiClient.Setup(client => client.RolesAllAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleResponse> { role });
        _apiClient.Setup(client => client.UsersAll2Async(org.CdpOrganisationGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationUserResponse> { user });

        var result = await _service.GetHomeViewModelAsync("org", CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationName.Should().Be("Org");
    }

    [Fact]
    public async Task GetApplicationsViewModelAsync_WhenOrganisationMissing_ReturnsNull()
    {
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Not Found", 404, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.GetApplicationsViewModelAsync("org", ct: CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetApplicationsViewModelAsync_WhenValid_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var allApps = new List<ApplicationResponse> { BuildApplicationResponse(10, "app-1") };
        var enabled = new List<OrganisationApplicationResponse> { BuildOrganisationApplicationResponse(org.Id, 10) };
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allApps);
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enabled);

        var result = await _service.GetApplicationsViewModelAsync("org", ct: CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrganisationName.Should().Be("Org");
    }

    [Fact]
    public async Task GetEnableApplicationViewModelAsync_WhenAppMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationResponse>());

        var result = await _service.GetEnableApplicationViewModelAsync("org", "missing", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEnableApplicationViewModelAsync_WhenValid_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var app = BuildApplicationResponse(10, "app-1");
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationResponse> { app });
        _apiClient.Setup(client => client.RolesAllAsync(app.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleResponse> { BuildRoleResponse(app.Id) });

        var result = await _service.GetEnableApplicationViewModelAsync("org", "app-1", CancellationToken.None);

        result.Should().NotBeNull();
        result!.ApplicationId.Should().Be(10);
    }

    [Fact]
    public async Task EnableApplicationAsync_WhenAppMissing_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationResponse>());

        var result = await _service.EnableApplicationAsync("org", "missing", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task EnableApplicationAsync_WhenValid_ReturnsTrue()
    {
        var org = BuildOrganisationResponse();
        var app = BuildApplicationResponse(10, "app-1");
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationResponse> { app });
        _apiClient.Setup(client => client.ApplicationsPOSTAsync(org.Id, It.IsAny<EnableApplicationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationApplicationResponse
            {
                Id = 1,
                OrganisationId = org.Id,
                ApplicationId = app.Id,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "system"
            });

        var result = await _service.EnableApplicationAsync("org", "app-1", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EnableApplicationAsync_WhenApiException_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        var app = BuildApplicationResponse(10, "app-1");
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationResponse> { app });
        _apiClient.Setup(client => client.ApplicationsPOSTAsync(org.Id, It.IsAny<EnableApplicationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.EnableApplicationAsync("org", "app-1", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetEnableSuccessViewModelAsync_WhenAppMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationResponse>());

        var result = await _service.GetEnableSuccessViewModelAsync("org", "missing", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetApplicationDetailsViewModelAsync_WhenAppMissing_ReturnsNull()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationApplicationResponse>());

        var result = await _service.GetApplicationDetailsViewModelAsync("org", "missing", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDisableApplicationViewModelAsync_WhenValid_ReturnsViewModel()
    {
        var org = BuildOrganisationResponse();
        var orgApp = BuildOrganisationApplicationResponse(org.Id, 10);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationApplicationResponse> { orgApp });

        var result = await _service.GetDisableApplicationViewModelAsync("org", "app-1", CancellationToken.None);

        result.Should().NotBeNull();
        result!.ApplicationId.Should().Be(10);
    }

    [Fact]
    public async Task DisableApplicationAsync_WhenAppMissing_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationApplicationResponse>());

        var result = await _service.DisableApplicationAsync("org", "missing", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DisableApplicationAsync_WhenApiException_ReturnsFalse()
    {
        var org = BuildOrganisationResponse();
        var orgApp = BuildOrganisationApplicationResponse(org.Id, 10);
        _apiClient.Setup(client => client.BySlugAsync("org", It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _apiClient.Setup(client => client.ApplicationsAllAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrganisationApplicationResponse> { orgApp });
        _apiClient.Setup(client => client.ApplicationsDELETEAsync(org.Id, orgApp.ApplicationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiException("Bad", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null));

        var result = await _service.DisableApplicationAsync("org", "app-1", CancellationToken.None);

        result.Should().BeFalse();
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

    private static OrganisationApplicationResponse BuildOrganisationApplicationResponse(int orgId, int appId) =>
        new()
        {
            Id = 1,
            OrganisationId = orgId,
            ApplicationId = appId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            Application = BuildApplicationResponse(appId, "app-1")
        };

    private static ApplicationResponse BuildApplicationResponse(int id, string clientId) =>
        new()
        {
            Id = id,
            Name = "App",
            ClientId = clientId,
            Description = "desc",
            Category = "cat",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private static RoleResponse BuildRoleResponse(int appId) =>
        new()
        {
            Id = 1,
            ApplicationId = appId,
            Name = "Role",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "system",
            Permissions = new List<PermissionResponse>
            {
                new()
                {
                    Id = 1,
                    ApplicationId = appId,
                    Name = "Read",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "system"
                }
            }
        };

    private static OrganisationUserResponse BuildOrganisationUserResponse(int orgId) =>
        new()
        {
            MembershipId = 1,
            OrganisationId = orgId,
            CdpPersonId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            OrganisationRole = OrganisationRole.Member,
            Status = UserStatus.Active,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationAssignments = []
        };
}
