using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Application.Users.Implementations;
using CO.CDP.UserManagement.App.Tests.TestFixtures;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Application.Users;

public class UserDetailsQueryServiceTests : AdapterTestFixture
{
    private readonly Mock<IUserManagementApiAdapter> _adapter = new();
    private readonly UserDetailsQueryService _sut;

    public UserDetailsQueryServiceTests()
        => _sut = new UserDetailsQueryService(_adapter.Object);

    [Fact]
    public async Task GetViewModelAsync_OrgNotFound_ReturnsNull()
    {
        _adapter.Setup(a => a.GetOrganisationBySlugAsync("slug", default))
            .ReturnsAsync((OrganisationResponse?)null);

        var result = await _sut.GetViewModelAsync("slug", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetViewModelAsync_UserNotFound_ReturnsNull()
    {
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, It.IsAny<Guid>(), default))
            .ReturnsAsync((OrganisationUserResponse?)null);

        var result = await _sut.GetViewModelAsync("test-org", Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetViewModelAsync_ValidUser_ReturnsMappedViewModel()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId: personId, firstName: "Jane", lastName: "Doe", email: "jane@example.com"));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(Array.Empty<OrganisationApplicationResponse>());

        var result = await _sut.GetViewModelAsync("test-org", personId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.CdpPersonId.Should().Be(personId);
        result.FullName.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task GetViewModelAsync_MapsUserApplicationsFromOrgApplicationList()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(
                personId: personId,
                applicationRoles: new[]
                {
                    new UserAssignmentResponse
                    {
                        OrganisationApplicationId = 10,
                        Id = 10,
                        UserOrganisationMembershipId = 0,
                        IsActive = true,
                        CreatedAt = DateTimeOffset.UtcNow
                    }
                }
            ));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(new[] { MakeApplication(orgAppId: 10, name: "App One") });

        var result = await _sut.GetViewModelAsync("test-org", personId, CancellationToken.None);

        result!.ApplicationAccess.Should().HaveCount(1);
        result.ApplicationAccess[0].ApplicationName.Should().Be("App One");
    }

    [Fact]
    public async Task GetViewModelAsync_UserWithNoApplicationAccess_ReturnsEmptyApplicationList()
    {
        var personId = Guid.NewGuid();
        SetupOrg();
        _adapter.Setup(a => a.GetUserAsync(OrgGuid, personId, default))
            .ReturnsAsync(MakeUser(personId: personId));
        _adapter.Setup(a => a.GetApplicationsAsync(OrgId, default))
            .ReturnsAsync(new[] { MakeApplication(orgAppId: 10, name: "App One") });

        var result = await _sut.GetViewModelAsync("test-org", personId, CancellationToken.None);

        result!.ApplicationAccess.Should().BeEmpty();
    }
}