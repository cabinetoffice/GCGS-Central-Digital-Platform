using CO.CDP.UserManagement.Core.Entities;
using CoreEntities = CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class ClaimsServiceTests
{
    private readonly Mock<IUserApplicationAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IUserOrganisationMembershipRepository> _membershipRepositoryMock;
    private readonly ClaimsService _service;

    public ClaimsServiceTests()
    {
        _assignmentRepositoryMock = new Mock<IUserApplicationAssignmentRepository>();
        _membershipRepositoryMock = new Mock<IUserOrganisationMembershipRepository>();
        _service = new ClaimsService(_assignmentRepositoryMock.Object, _membershipRepositoryMock.Object);
    }

    [Fact]
    public async Task GetUserClaimsAsync_WithNoMemberships_ReturnsEmptyClaims()
    {
        // Arrange
        var userId = "user123";
        _membershipRepositoryMock.Setup(r => r.GetByUserPrincipalIdAsync(userId, default))
            .ReturnsAsync(new List<UserOrganisationMembership>());
        _assignmentRepositoryMock.Setup(r => r.GetAssignmentsForClaimsAsync(userId, default))
            .ReturnsAsync(new List<UserApplicationAssignment>());

        // Act
        var result = await _service.GetUserClaimsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserPrincipalId.Should().Be(userId);
        result.Organisations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserClaimsAsync_WithMemberships_ReturnsCompleteClaimsStructure()
    {
        // Arrange
        var userId = "user123";
        var orgGuid = Guid.NewGuid();
        var appGuid = Guid.NewGuid();

        var org = new CoreEntities.Organisation
        {
            Id = 1,
            Name = "Test Org",
            Slug = "test-org",
            CdpOrganisationGuid = orgGuid,
            IsActive = true
        };

        var membership = new UserOrganisationMembership
        {
            Id = 1,
            UserPrincipalId = userId,
            OrganisationId = 1,
            Organisation = org,
            OrganisationRoleId = (int)OrganisationRole.Admin,
            IsActive = true
        };

        var app = new Application
        {
            Id = 1,
            Guid = appGuid,
            Name = "Test App",
            ClientId = "test-app-client",
            IsActive = true
        };

        var permission = new ApplicationPermission
        {
            Id = 1,
            ApplicationId = 1,
            Name = "read",
            IsActive = true
        };

        var role = new ApplicationRole
        {
            Id = 1,
            ApplicationId = 1,
            Name = "Admin",
            IsActive = true,
            Permissions = new List<ApplicationPermission> { permission }
        };

        var orgApp = new OrganisationApplication
        {
            Id = 1,
            OrganisationId = 1,
            ApplicationId = 1,
            Application = app,
            Organisation = org,
            IsActive = true
        };

        var assignment = new UserApplicationAssignment
        {
            Id = 1,
            UserOrganisationMembershipId = 1,
            UserOrganisationMembership = membership,
            OrganisationApplicationId = 1,
            OrganisationApplication = orgApp,
            IsActive = true,
            Roles = new List<ApplicationRole> { role }
        };

        _membershipRepositoryMock.Setup(r => r.GetByUserPrincipalIdAsync(userId, default))
            .ReturnsAsync(new List<UserOrganisationMembership> { membership });
        _assignmentRepositoryMock.Setup(r => r.GetAssignmentsForClaimsAsync(userId, default))
            .ReturnsAsync(new List<UserApplicationAssignment> { assignment });

        // Act
        var result = await _service.GetUserClaimsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserPrincipalId.Should().Be(userId);
        result.Organisations.Should().HaveCount(1);

        var orgClaim = result.Organisations.First();
        orgClaim.OrganisationId.Should().Be(orgGuid);
        orgClaim.OrganisationName.Should().Be("Test Org");
        orgClaim.OrganisationRole.Should().Be("Admin");

        orgClaim.Applications.Should().HaveCount(1);
        var appClaim = orgClaim.Applications.First();
        appClaim.ApplicationId.Should().Be(appGuid);
        appClaim.ApplicationName.Should().Be("Test App");
        appClaim.ClientId.Should().Be("test-app-client");
        appClaim.Roles.Should().Contain("Admin");
        appClaim.Permissions.Should().Contain("read");
    }
}
