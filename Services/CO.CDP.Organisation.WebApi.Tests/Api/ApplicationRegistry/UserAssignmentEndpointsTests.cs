using System.Net;
using System.Net.Http.Json;
using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api.ApplicationRegistry;

/// <summary>
/// Tests for <c>UserAssignmentEndpoints</c> (Phase 3A — User Assignments).
/// All operations require OrgAdmin for the relevant organisation.
/// </summary>
public class UserAssignmentEndpointsTests
{
    private readonly Mock<IUserAssignmentRepository> _assignmentRepo = new();
    private readonly Mock<IApplicationRepository>    _applicationRepo = new();

    // ── GET /api/organisations/{orgId}/applications/{appId}/users ─────────

    [Fact]
    public async Task GetAssignments_Returns_Ok_With_Assignments()
    {
        var orgId  = Guid.NewGuid();
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permId = Guid.NewGuid();

        var perm = new ApplicationPermission
            { Id = permId, ApplicationId = appId, Name = "submit:notice" };
        var role = new ApplicationRole
        {
            Id = roleId, ApplicationId = appId, Name = "Buyer",
            RolePermissions = [new RolePermission { RoleId = roleId, PermissionId = permId, Permission = perm }]
        };
        var assignment = new UserApplicationAssignment
        {
            Id              = Guid.NewGuid(),
            UserPrincipalId = "urn:test:user",
            ApplicationId   = appId,
            OrganisationId  = orgId,
            AssignedBy      = "system",
            RoleAssignments = [new UserRoleAssignment { RoleId = roleId, Role = role }]
        };

        _assignmentRepo.Setup(r => r.GetAssignmentsAsync(orgId, appId)).ReturnsAsync([assignment]);

        var response = await OrgAdminClient(orgId)
            .GetAsync($"/api/organisations/{orgId}/applications/{appId}/users");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<UserAssignmentDto[]>();
        result.Should().HaveCount(1);
        result![0].UserPrincipalId.Should().Be("urn:test:user");
        result[0].Roles.Should().HaveCount(1);
        result[0].Roles.First().Name.Should().Be("Buyer");
    }

    [Theory]
    [InlineData(OK,        true)]   // OrgAdmin
    [InlineData(Forbidden, false)]  // No auth
    public async Task GetAssignments_Authorization(HttpStatusCode expected, bool orgAdmin)
    {
        var orgId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        _assignmentRepo.Setup(r => r.GetAssignmentsAsync(orgId, appId)).ReturnsAsync([]);

        var client   = orgAdmin ? OrgAdminClient(orgId) : UnauthorizedClient();
        var response = await client.GetAsync($"/api/organisations/{orgId}/applications/{appId}/users");
        response.StatusCode.Should().Be(expected);
    }

    // ── POST /api/organisations/{orgId}/applications/{appId}/users ────────

    [Fact]
    public async Task CreateAssignment_Returns_Created()
    {
        var orgId  = Guid.NewGuid();
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var command = new CreateUserAssignment("urn:new:user", [roleId]);
        var created = new UserApplicationAssignment
        {
            Id = Guid.NewGuid(), UserPrincipalId = "urn:new:user",
            ApplicationId = appId, OrganisationId = orgId, AssignedBy = "system"
        };

        _applicationRepo
            .Setup(r => r.GetRolesAsync(appId))
            .ReturnsAsync([new ApplicationRole { Id = roleId, ApplicationId = appId, Name = "Role" }]);

        _assignmentRepo.Setup(r => r.CreateAssignmentAsync(It.IsAny<UserApplicationAssignment>()))
            .ReturnsAsync(created);

        var response = await OrgAdminClient(orgId)
            .PostAsJsonAsync($"/api/organisations/{orgId}/applications/{appId}/users", command);

        response.Should().HaveStatusCode(Created);
    }

    [Theory]
    [InlineData(Created,   true)]
    [InlineData(Forbidden, false)]
    public async Task CreateAssignment_Authorization(HttpStatusCode expected, bool orgAdmin)
    {
        var orgId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        _assignmentRepo.Setup(r => r.CreateAssignmentAsync(It.IsAny<UserApplicationAssignment>()))
            .ReturnsAsync(new UserApplicationAssignment
            {
                Id = Guid.NewGuid(), UserPrincipalId = "urn:u", ApplicationId = appId,
                OrganisationId = orgId, AssignedBy = "system"
            });

        var client   = orgAdmin ? OrgAdminClient(orgId) : UnauthorizedClient();
        var response = await client.PostAsJsonAsync(
            $"/api/organisations/{orgId}/applications/{appId}/users",
            new CreateUserAssignment("urn:u", null));
        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task CreateAssignment_Returns_BadRequest_WhenRoleIdBelongsToDifferentApplication()
    {
        var orgId         = Guid.NewGuid();
        var appId         = Guid.NewGuid();
        var foreignRoleId = Guid.NewGuid(); // belongs to a different application

        // Application repo returns an empty role list for appId — the foreign role is absent
        _applicationRepo
            .Setup(r => r.GetRolesAsync(appId))
            .ReturnsAsync(Array.Empty<ApplicationRole>());

        var command = new CreateUserAssignment("urn:bad:user", [foreignRoleId]);

        var response = await OrgAdminClient(orgId)
            .PostAsJsonAsync($"/api/organisations/{orgId}/applications/{appId}/users", command);

        response.Should().HaveStatusCode(BadRequest);
    }

    // ── PUT /api/organisations/{orgId}/applications/{appId}/users/{userId} ──

    [Fact]
    public async Task UpdateAssignment_Returns_NoContent_WhenFound()
    {
        var orgId  = Guid.NewGuid();
        var appId  = Guid.NewGuid();
        var userId = "urn:existing:user";

        var existing = new UserApplicationAssignment
        {
            Id = Guid.NewGuid(), UserPrincipalId = userId, ApplicationId = appId,
            OrganisationId = orgId, AssignedBy = "system"
        };

        _assignmentRepo.Setup(r => r.GetAssignmentAsync(orgId, appId, userId)).ReturnsAsync(existing);
        _assignmentRepo.Setup(r => r.UpdateAssignmentAsync(existing)).Returns(Task.CompletedTask);

        var response = await OrgAdminClient(orgId)
            .PutAsJsonAsync(
                $"/api/organisations/{orgId}/applications/{appId}/users/{userId}",
                new UpdateUserAssignment([Guid.NewGuid()]));

        response.Should().HaveStatusCode(NoContent);
    }

    [Fact]
    public async Task UpdateAssignment_Returns_NotFound_WhenMissing()
    {
        var orgId  = Guid.NewGuid();
        var appId  = Guid.NewGuid();
        var userId = "urn:missing:user";

        _assignmentRepo.Setup(r => r.GetAssignmentAsync(orgId, appId, userId))
            .ReturnsAsync((UserApplicationAssignment?)null);

        var response = await OrgAdminClient(orgId)
            .PutAsJsonAsync(
                $"/api/organisations/{orgId}/applications/{appId}/users/{userId}",
                new UpdateUserAssignment([]));

        response.Should().HaveStatusCode(NotFound);
    }

    [Theory]
    [InlineData(NoContent, true)]
    [InlineData(Forbidden, false)]
    public async Task UpdateAssignment_Authorization(HttpStatusCode expected, bool orgAdmin)
    {
        var orgId  = Guid.NewGuid();
        var appId  = Guid.NewGuid();
        var userId = "urn:u";
        var existing = new UserApplicationAssignment
        {
            Id = Guid.NewGuid(), UserPrincipalId = userId, ApplicationId = appId,
            OrganisationId = orgId, AssignedBy = "system"
        };

        _assignmentRepo.Setup(r => r.GetAssignmentAsync(orgId, appId, userId)).ReturnsAsync(existing);
        _assignmentRepo.Setup(r => r.UpdateAssignmentAsync(existing)).Returns(Task.CompletedTask);

        var client   = orgAdmin ? OrgAdminClient(orgId) : UnauthorizedClient();
        var response = await client.PutAsJsonAsync(
            $"/api/organisations/{orgId}/applications/{appId}/users/{userId}",
            new UpdateUserAssignment([]));
        response.StatusCode.Should().Be(expected);
    }

    // ── DELETE /api/organisations/{orgId}/applications/{appId}/users/{userId} ──

    [Fact]
    public async Task RevokeAssignment_Returns_NoContent()
    {
        var orgId  = Guid.NewGuid();
        var appId  = Guid.NewGuid();
        var userId = "urn:revoke:user";

        _assignmentRepo.Setup(r => r.RevokeAssignmentAsync(orgId, appId, userId)).Returns(Task.CompletedTask);

        var response = await OrgAdminClient(orgId)
            .DeleteAsync($"/api/organisations/{orgId}/applications/{appId}/users/{userId}");

        response.Should().HaveStatusCode(NoContent);
        _assignmentRepo.Verify(r => r.RevokeAssignmentAsync(orgId, appId, userId), Times.Once);
    }

    [Theory]
    [InlineData(NoContent, true)]
    [InlineData(Forbidden, false)]
    public async Task RevokeAssignment_Authorization(HttpStatusCode expected, bool orgAdmin)
    {
        var orgId  = Guid.NewGuid();
        var appId  = Guid.NewGuid();
        var userId = "urn:u";
        _assignmentRepo.Setup(r => r.RevokeAssignmentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var client   = orgAdmin ? OrgAdminClient(orgId) : UnauthorizedClient();
        var response = await client.DeleteAsync(
            $"/api/organisations/{orgId}/applications/{appId}/users/{userId}");
        response.StatusCode.Should().Be(expected);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private HttpClient OrgAdminClient(Guid orgId) =>
        AppRegistryTestFactory.OrgAdmin(orgId, services =>
        {
            services.AddScoped<IUserAssignmentRepository>(_ => _assignmentRepo.Object);
            services.AddScoped<IApplicationRepository>(_ => _applicationRepo.Object);
        });

    private HttpClient UnauthorizedClient() =>
        AppRegistryTestFactory.Unauthorized(services =>
        {
            services.AddScoped<IUserAssignmentRepository>(_ => _assignmentRepo.Object);
            services.AddScoped<IApplicationRepository>(_ => _applicationRepo.Object);
        });
}
