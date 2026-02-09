using CO.CDP.UserManagement.Core.Entities;
using OrganisationEntity = CO.CDP.UserManagement.Core.Entities.Organisation;
using CO.CDP.ApplicationRegistry.Shared.Enums;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace CO.CDP.UserManagement.UnitTests.Services;

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
        result.OrganisationMemberships.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserClaimsAsync_WithMemberships_ReturnsCompleteClaimsStructure()
    {
        // Arrange
        var userId = "user123";

        var org = new OrganisationEntity
        {
            Id = 1,
            Name = "Test Org",
            Slug = "test-org",
            CdpOrganisationGuid = Guid.NewGuid(),
            IsActive = true
        };

        var membership = new UserOrganisationMembership
        {
            Id = 1,
            UserPrincipalId = userId,
            OrganisationId = 1,
            Organisation = org,
            OrganisationRole = OrganisationRole.Admin,
            IsActive = true
        };

        var app = new Application
        {
            Id = 1,
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
        result.OrganisationMemberships.Should().HaveCount(1);

        var orgClaim = result.OrganisationMemberships.First();
        orgClaim.OrganisationId.Should().Be(1);
        orgClaim.OrganisationName.Should().Be("Test Org");
        orgClaim.OrganisationSlug.Should().Be("test-org");
        orgClaim.OrganisationRole.Should().Be("Admin");

        orgClaim.ApplicationAssignments.Should().HaveCount(1);
        var appClaim = orgClaim.ApplicationAssignments.First();
        appClaim.ApplicationId.Should().Be(1);
        appClaim.ApplicationName.Should().Be("Test App");
        appClaim.ClientId.Should().Be("test-app-client");
        appClaim.Roles.Should().Contain("Admin");
        appClaim.Permissions.Should().Contain("read");
    }
}
