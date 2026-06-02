using System.Net;
using System.Net.Http.Json;
using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using static System.Net.HttpStatusCode;
using Application = CO.CDP.ApplicationRegistry.Persistence.Entities.Application;
using AppPerm    = CO.CDP.ApplicationRegistry.Persistence.Entities.ApplicationPermission;
using AppRole    = CO.CDP.ApplicationRegistry.Persistence.Entities.ApplicationRole;

namespace CO.CDP.Organisation.WebApi.Tests.Api.ApplicationRegistry;

/// <summary>
/// Tests for <c>ApplicationEndpoints</c> (Phase 2B — Application/Roles coverage).
/// All write operations require PlatformAdmin. Read operations are open.
/// </summary>
public class ApplicationEndpointsTests
{
    private readonly Mock<IUseCase<bool, IEnumerable<ApplicationDto>>>       _getApplicationsUseCase = new();
    private readonly Mock<IUseCase<Guid, ApplicationDto?>>                   _getApplicationUseCase  = new();
    private readonly Mock<IUseCase<CreateApplication, ApplicationDto>>       _registerAppUseCase     = new();
    private readonly Mock<IUseCase<(Guid, UpdateApplication), bool>>         _updateAppUseCase       = new();
    private readonly Mock<IUseCase<(Guid, CreatePermission), PermissionDto>> _createPermUseCase      = new();
    private readonly Mock<IApplicationRepository>                            _appRepo                = new();

    // ── GET /api/applications ──────────────────────────────────────────────

    [Fact]
    public async Task GetApplications_Returns_Ok_With_List()
    {
        var expected = new[] { GivenApplicationDto(Guid.NewGuid(), "FTS") };
        _getApplicationsUseCase.Setup(uc => uc.Execute(true)).ReturnsAsync(expected);

        var response = await PlatformAdminClient().GetAsync("/api/applications");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<ApplicationDto[]>();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("FTS");
    }

    [Theory]
    [InlineData(OK,        true)]   // PlatformAdmin
    [InlineData(Forbidden, false)]  // No auth
    public async Task GetApplications_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        _getApplicationsUseCase.Setup(uc => uc.Execute(It.IsAny<bool>())).ReturnsAsync([]);

        var client = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.GetAsync("/api/applications");
        response.StatusCode.Should().Be(expected);
    }

    // ── POST /api/applications ────────────────────────────────────────────

    [Fact]
    public async Task CreateApplication_Returns_Created_With_Location()
    {
        var command = new CreateApplication("FindATender", "fat-app", "Procurement platform", null);
        var dto     = GivenApplicationDto(Guid.NewGuid(), command.Name);
        _registerAppUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(dto);

        var response = await PlatformAdminClient().PostAsJsonAsync("/api/applications", command);

        response.Should().HaveStatusCode(Created);
        response.Headers.Location?.ToString().Should().Contain(dto.Id.ToString());
        var result = await response.Content.ReadFromJsonAsync<ApplicationDto>();
        result!.Name.Should().Be("FindATender");
    }

    [Theory]
    [InlineData(Created,   true)]   // PlatformAdmin → Created
    [InlineData(Forbidden, false)]  // No auth → Forbidden
    public async Task CreateApplication_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        var command = new CreateApplication("App", "app-client", null, null);
        _registerAppUseCase.Setup(uc => uc.Execute(It.IsAny<CreateApplication>()))
            .ReturnsAsync(GivenApplicationDto(Guid.NewGuid(), "App"));

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.PostAsJsonAsync("/api/applications", command);
        response.StatusCode.Should().Be(expected);
    }

    // ── GET /api/applications/{appId} ─────────────────────────────────────

    [Fact]
    public async Task GetApplication_Returns_Ok_WhenFound()
    {
        var appId = Guid.NewGuid();
        _getApplicationUseCase.Setup(uc => uc.Execute(appId))
            .ReturnsAsync(GivenApplicationDto(appId, "FAT"));

        var response = await PlatformAdminClient().GetAsync($"/api/applications/{appId}");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<ApplicationDto>();
        result!.Id.Should().Be(appId);
    }

    [Fact]
    public async Task GetApplication_Returns_NotFound_WhenMissing()
    {
        var appId = Guid.NewGuid();
        _getApplicationUseCase.Setup(uc => uc.Execute(appId)).ReturnsAsync((ApplicationDto?)null);

        var response = await PlatformAdminClient().GetAsync($"/api/applications/{appId}");
        response.Should().HaveStatusCode(NotFound);
    }

    // ── PUT /api/applications/{appId} ─────────────────────────────────────

    [Theory]
    [InlineData(NoContent, true)]
    [InlineData(Forbidden, false)]
    public async Task UpdateApplication_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        var appId   = Guid.NewGuid();
        var command = new UpdateApplication("Updated", null, null, true);
        _updateAppUseCase.Setup(uc => uc.Execute(It.IsAny<(Guid, UpdateApplication)>())).ReturnsAsync(true);

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.PutAsJsonAsync($"/api/applications/{appId}", command);
        response.StatusCode.Should().Be(expected);
    }

    // ── DELETE /api/applications/{appId} ──────────────────────────────────

    [Theory]
    [InlineData(NoContent, true)]
    [InlineData(Forbidden, false)]
    public async Task DeleteApplication_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        var appId = Guid.NewGuid();
        _updateAppUseCase.Setup(uc => uc.Execute(It.IsAny<(Guid, UpdateApplication)>())).ReturnsAsync(true);

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.DeleteAsync($"/api/applications/{appId}");
        response.StatusCode.Should().Be(expected);
    }

    // ── GET /api/applications/{appId}/permissions ─────────────────────────

    [Fact]
    public async Task GetPermissions_Returns_Ok_With_List()
    {
        var appId = Guid.NewGuid();
        var perms = new[] { new AppPerm { Id = Guid.NewGuid(), ApplicationId = appId, Name = "submit:notice" } };
        _appRepo.Setup(r => r.GetPermissionsAsync(appId)).ReturnsAsync(perms);

        var response = await PlatformAdminClient().GetAsync($"/api/applications/{appId}/permissions");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<PermissionDto[]>();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("submit:notice");
    }

    // ── POST /api/applications/{appId}/permissions ────────────────────────

    [Fact]
    public async Task CreatePermission_Returns_Created()
    {
        var appId   = Guid.NewGuid();
        var command = new CreatePermission("publish:contract", null);
        var dto     = new PermissionDto(Guid.NewGuid(), appId, "publish:contract", null);
        _createPermUseCase.Setup(uc => uc.Execute(It.IsAny<(Guid, CreatePermission)>())).ReturnsAsync(dto);

        var response = await PlatformAdminClient().PostAsJsonAsync($"/api/applications/{appId}/permissions", command);

        response.Should().HaveStatusCode(Created);
        var result = await response.Content.ReadFromJsonAsync<PermissionDto>();
        result!.Name.Should().Be("publish:contract");
    }

    [Theory]
    [InlineData(Created,   true)]
    [InlineData(Forbidden, false)]
    public async Task CreatePermission_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        var appId   = Guid.NewGuid();
        var command = new CreatePermission("read:data", null);
        _createPermUseCase.Setup(uc => uc.Execute(It.IsAny<(Guid, CreatePermission)>()))
            .ReturnsAsync(new PermissionDto(Guid.NewGuid(), appId, "read:data", null));

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.PostAsJsonAsync($"/api/applications/{appId}/permissions", command);
        response.StatusCode.Should().Be(expected);
    }

    // ── GET /api/applications/{appId}/roles ───────────────────────────────

    [Fact]
    public async Task GetRoles_Returns_Ok_With_Roles_And_Permissions()
    {
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var permId = Guid.NewGuid();

        var perm = new AppPerm { Id = permId, ApplicationId = appId, Name = "submit:notice" };
        var role = new AppRole
        {
            Id            = roleId,
            ApplicationId = appId,
            Name          = "Buyer",
            RolePermissions = [new RolePermission { RoleId = roleId, PermissionId = permId, Permission = perm }]
        };

        _appRepo.Setup(r => r.GetRolesAsync(appId)).ReturnsAsync([role]);

        var response = await PlatformAdminClient().GetAsync($"/api/applications/{appId}/roles");

        response.Should().HaveStatusCode(OK);
        var result = await response.Content.ReadFromJsonAsync<RoleDto[]>();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("Buyer");
        result[0].Permissions.Should().HaveCount(1);
        result[0].Permissions.First().Name.Should().Be("submit:notice");
    }

    // ── POST /api/applications/{appId}/roles ──────────────────────────────

    [Fact]
    public async Task CreateRole_Returns_Created()
    {
        var appId   = Guid.NewGuid();
        var command = new CreateRole("Supplier", "Supplier role");
        var newRole = new AppRole { Id = Guid.NewGuid(), ApplicationId = appId, Name = "Supplier" };

        _appRepo.Setup(r => r.CreateRoleAsync(It.IsAny<AppRole>())).ReturnsAsync(newRole);

        var response = await PlatformAdminClient().PostAsJsonAsync($"/api/applications/{appId}/roles", command);

        response.Should().HaveStatusCode(Created);
        var result = await response.Content.ReadFromJsonAsync<RoleDto>();
        result!.Name.Should().Be("Supplier");
    }

    [Theory]
    [InlineData(Created,   true)]
    [InlineData(Forbidden, false)]
    public async Task CreateRole_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        var appId = Guid.NewGuid();
        _appRepo.Setup(r => r.CreateRoleAsync(It.IsAny<AppRole>()))
            .ReturnsAsync(new AppRole { Id = Guid.NewGuid(), ApplicationId = appId, Name = "R" });

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.PostAsJsonAsync($"/api/applications/{appId}/roles",
            new CreateRole("R", null));
        response.StatusCode.Should().Be(expected);
    }

    // ── PUT /api/applications/{appId}/roles/{roleId}/permissions ─────────

    [Fact]
    public async Task SetRolePermissions_Returns_NoContent()
    {
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var body   = new SetRolePermissions([Guid.NewGuid()]);

        _appRepo.Setup(r => r.SetRolePermissionsAsync(roleId, body.PermissionIds))
            .Returns(Task.CompletedTask);

        var response = await PlatformAdminClient()
            .PutAsJsonAsync($"/api/applications/{appId}/roles/{roleId}/permissions", body);

        response.Should().HaveStatusCode(NoContent);
    }

    [Theory]
    [InlineData(NoContent, true)]
    [InlineData(Forbidden, false)]
    public async Task SetRolePermissions_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _appRepo.Setup(r => r.SetRolePermissionsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
            .Returns(Task.CompletedTask);

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.PutAsJsonAsync(
            $"/api/applications/{appId}/roles/{roleId}/permissions",
            new SetRolePermissions([]));
        response.StatusCode.Should().Be(expected);
    }

    // ── UPDATE ROLE ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRole_Returns_NoContent_WhenRoleExists()
    {
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var role   = new AppRole { Id = roleId, ApplicationId = appId, Name = "Admin" };

        _appRepo.Setup(r => r.GetRoleByIdAsync(roleId)).ReturnsAsync(role);
        _appRepo.Setup(r => r.UpdateRoleAsync(role)).Returns(Task.CompletedTask);

        var response = await PlatformAdminClient()
            .PutAsJsonAsync($"/api/applications/{appId}/roles/{roleId}", new UpdateRole("SuperAdmin", null, null));

        response.Should().HaveStatusCode(NoContent);
    }

    [Fact]
    public async Task UpdateRole_Returns_NotFound_WhenRoleMissing()
    {
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _appRepo.Setup(r => r.GetRoleByIdAsync(roleId)).ReturnsAsync((AppRole?)null);

        var response = await PlatformAdminClient()
            .PutAsJsonAsync($"/api/applications/{appId}/roles/{roleId}", new UpdateRole("X", null, null));

        response.Should().HaveStatusCode(NotFound);
    }

    // ── DELETE ROLE ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(NoContent, true)]
    [InlineData(Forbidden, false)]
    public async Task DeleteRole_Authorization(HttpStatusCode expected, bool platformAdmin)
    {
        var appId  = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _appRepo.Setup(r => r.DeleteRoleAsync(roleId)).Returns(Task.CompletedTask);

        var client   = platformAdmin ? PlatformAdminClient() : UnauthorizedClient();
        var response = await client.DeleteAsync($"/api/applications/{appId}/roles/{roleId}");
        response.StatusCode.Should().Be(expected);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private HttpClient PlatformAdminClient() =>
        AppRegistryTestFactory.PlatformAdmin(services =>
        {
            services.AddScoped(_ => _getApplicationsUseCase.Object);
            services.AddScoped(_ => _getApplicationUseCase.Object);
            services.AddScoped(_ => _registerAppUseCase.Object);
            services.AddScoped(_ => _updateAppUseCase.Object);
            services.AddScoped(_ => _createPermUseCase.Object);
            services.AddScoped<IApplicationRepository>(_ => _appRepo.Object);
        });

    private HttpClient UnauthorizedClient() =>
        AppRegistryTestFactory.Unauthorized(services =>
        {
            services.AddScoped(_ => _getApplicationsUseCase.Object);
            services.AddScoped(_ => _getApplicationUseCase.Object);
            services.AddScoped(_ => _registerAppUseCase.Object);
            services.AddScoped(_ => _updateAppUseCase.Object);
            services.AddScoped(_ => _createPermUseCase.Object);
            services.AddScoped<IApplicationRepository>(_ => _appRepo.Object);
        });

    private static ApplicationDto GivenApplicationDto(Guid id, string name) =>
        new(id, name, $"{name.ToLower()}-client", null, null, true,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
