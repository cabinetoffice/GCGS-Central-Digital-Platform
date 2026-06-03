using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;
using FluentAssertions;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Tests;

[Collection("MongoDB")]
public class MongoApplicationRepositoryTests
{
    private readonly MongoApplicationRepository _repo;
    private readonly MongoDB.MongoAppRegistryDatabase _db;

    public MongoApplicationRepositoryTests(MongoDbFixture fixture)
    {
        // Each test class gets its own isolated database.
        _db   = fixture.CreateDatabase("app_tests_application");
        _repo = new MongoApplicationRepository(_db, fixture.StubAudit(), fixture.StubCurrentUser());
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Application NewApplication(string? clientId = null, string? name = null) => new()
    {
        Name        = name ?? $"App-{Guid.NewGuid():N}",
        ClientId    = clientId ?? $"client-{Guid.NewGuid():N}",
        Description = "Test application",
        IsActive    = true
    };

    private static ApplicationPermission NewPermission(Guid appId, string? name = null) => new()
    {
        ApplicationId = appId,
        Name          = name ?? $"perm:{Guid.NewGuid():N}",
        Description   = "A test permission",
        IsActive      = true
    };

    private static ApplicationRole NewRole(Guid appId, string? name = null) => new()
    {
        ApplicationId = appId,
        Name          = name ?? $"role:{Guid.NewGuid():N}",
        Description   = "A test role",
        IsActive      = true
    };

    // ── Tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsApplication()
    {
        var app = NewApplication();

        var created = await _repo.CreateAsync(app);

        var fetched = await _repo.GetByIdAsync(created.Id);
        fetched.Should().NotBeNull();
        fetched!.ClientId.Should().Be(app.ClientId);
        fetched.Name.Should().Be(app.Name);
        fetched.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenApplicationIsInactive()
    {
        var app = NewApplication();
        await _repo.CreateAsync(app);

        // Soft-delete by flipping IsActive.
        app.IsActive = false;
        await _repo.UpdateAsync(app);

        var result = await _repo.GetByIdAsync(app.Id);
        result.Should().BeNull("GetByIdAsync must honour the soft-delete flag");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveApplications()
    {
        var active   = NewApplication(name: "Active-App");
        var inactive = NewApplication(name: "Inactive-App");

        await _repo.CreateAsync(active);
        await _repo.CreateAsync(inactive);

        inactive.IsActive = false;
        await _repo.UpdateAsync(inactive);

        var results = (await _repo.GetAllAsync()).ToList();

        results.Should().Contain(a => a.Id == active.Id);
        results.Should().NotContain(a => a.Id == inactive.Id);
    }

    [Fact]
    public async Task CreateRoleAsync_AddsRoleToApplication()
    {
        var app = await _repo.CreateAsync(NewApplication());
        var role = NewRole(app.Id, "Admin");

        await _repo.CreateRoleAsync(role);

        var roles = (await _repo.GetRolesAsync(app.Id)).ToList();
        roles.Should().ContainSingle(r => r.Name == "Admin");
    }

    [Fact]
    public async Task DeleteRoleAsync_SoftDeletesRole_RoleRemainsInDocumentWithIsActiveFalse()
    {
        var app  = await _repo.CreateAsync(NewApplication());
        var role = NewRole(app.Id, "ToDelete");
        await _repo.CreateRoleAsync(role);

        await _repo.DeleteRoleAsync(role.Id);

        // GetRoleByIdAsync must return null for soft-deleted roles.
        var byId = await _repo.GetRoleByIdAsync(role.Id);
        byId.Should().BeNull("soft-deleted role must not be visible through GetRoleByIdAsync");

        // But the raw document must still contain the role with isActive = false.
        var raw = await _db.Applications
            .Find(a => a.Id == app.Id)
            .FirstOrDefaultAsync();

        raw.Should().NotBeNull();
        var rawRole = raw!.Roles.FirstOrDefault(r => r.Id == role.Id);
        rawRole.Should().NotBeNull("the role document must still exist in the embedded array");
        rawRole!.IsActive.Should().BeFalse("soft-delete sets isActive=false, not a hard delete");
    }

    [Fact]
    public async Task SetRolePermissionsAsync_ReplacesPermissionSet()
    {
        var app = await _repo.CreateAsync(NewApplication());

        // Add two permissions to the application.
        var perm1 = NewPermission(app.Id, "read");
        var perm2 = NewPermission(app.Id, "write");
        await _repo.CreatePermissionAsync(perm1);
        await _repo.CreatePermissionAsync(perm2);

        // Add a role.
        var role = NewRole(app.Id, "Editor");
        await _repo.CreateRoleAsync(role);

        // Assign both permissions.
        await _repo.SetRolePermissionsAsync(role.Id, [perm1.Id, perm2.Id]);

        var fetched = await _repo.GetRoleByIdAsync(role.Id);
        fetched.Should().NotBeNull();
        fetched!.RolePermissions.Should().HaveCount(2);
        fetched.RolePermissions.Select(rp => rp.PermissionId)
            .Should().BeEquivalentTo([perm1.Id, perm2.Id]);

        // Replace with only perm1.
        await _repo.SetRolePermissionsAsync(role.Id, [perm1.Id]);

        var updated = await _repo.GetRoleByIdAsync(role.Id);
        updated!.RolePermissions.Should().ContainSingle(rp => rp.PermissionId == perm1.Id);
    }

    [Fact]
    public async Task CreateAsync_UniqueClientId_ThrowsOnDuplicate()
    {
        var clientId = $"client-{Guid.NewGuid():N}";
        await _repo.CreateAsync(NewApplication(clientId: clientId));

        // Inserting a second document with the same ClientId must throw due to the unique index.
        Func<Task> act = () => _repo.CreateAsync(NewApplication(clientId: clientId));

        await act.Should().ThrowAsync<MongoWriteException>(
            "the unique index idx_application_clientId must reject duplicate ClientId values");
    }
}
