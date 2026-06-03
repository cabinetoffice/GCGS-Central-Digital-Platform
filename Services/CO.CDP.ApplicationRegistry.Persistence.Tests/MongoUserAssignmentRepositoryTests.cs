using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;
using FluentAssertions;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Tests;

[Collection("MongoDB")]
public class MongoUserAssignmentRepositoryTests
{
    private readonly MongoUserAssignmentRepository _repo;
    private readonly MongoDB.MongoAppRegistryDatabase _db;

    // Stable IDs shared within this class's isolated database.
    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _appId = Guid.NewGuid();

    public MongoUserAssignmentRepositoryTests(MongoDbFixture fixture)
    {
        _db   = fixture.CreateDatabase("app_tests_assignment");
        _repo = new MongoUserAssignmentRepository(_db, fixture.StubAudit(), fixture.StubCurrentUser());
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private UserApplicationAssignment NewAssignment(string? upn = null) => new()
    {
        UserPrincipalId = upn ?? $"user-{Guid.NewGuid():N}@test",
        ApplicationId   = _appId,
        OrganisationId  = _orgId,
        AssignedBy      = "test-user",
        IsActive        = true
    };

    /// <summary>
    /// Seeds a minimal Application document so HydrateRoleAssignments has something to
    /// resolve.  Without a matching application document the hydration falls back to an
    /// empty list, which does not affect the tests below that only check IsActive /
    /// unique-index behaviour.
    /// </summary>
    private async Task SeedApplicationAsync()
    {
        var exists = await _db.Applications
            .Find(a => a.Id == _appId)
            .FirstOrDefaultAsync();

        if (exists != null) return;

        await _db.Applications.InsertOneAsync(new Application
        {
            Id       = _appId,
            Name     = "SeedApp",
            ClientId = $"client-{_appId:N}",
            IsActive = true
        });
    }

    // ── Tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAssignmentAsync_PersistsAssignment()
    {
        await SeedApplicationAsync();
        var assignment = NewAssignment("persist-user@test");

        var created = await _repo.CreateAssignmentAsync(assignment);

        var fetched = await _repo.GetAssignmentAsync(_orgId, _appId, "persist-user@test");
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAssignmentAsync_SetsIsActiveFalse()
    {
        await SeedApplicationAsync();
        var assignment = NewAssignment("revoke-user@test");
        await _repo.CreateAssignmentAsync(assignment);

        await _repo.RevokeAssignmentAsync(_orgId, _appId, "revoke-user@test");

        // Raw document must exist but with IsActive = false.
        var raw = await _db.UserAssignments
            .Find(a => a.OrganisationId  == _orgId
                    && a.ApplicationId   == _appId
                    && a.UserPrincipalId == "revoke-user@test")
            .FirstOrDefaultAsync();

        raw.Should().NotBeNull();
        raw!.IsActive.Should().BeFalse("RevokeAssignment is a soft-delete");
    }

    [Fact]
    public async Task GetAssignmentAsync_ReturnsNull_WhenRevoked()
    {
        await SeedApplicationAsync();
        var assignment = NewAssignment("revoked-lookup@test");
        await _repo.CreateAssignmentAsync(assignment);
        await _repo.RevokeAssignmentAsync(_orgId, _appId, "revoked-lookup@test");

        var result = await _repo.GetAssignmentAsync(_orgId, _appId, "revoked-lookup@test");

        result.Should().BeNull("GetAssignmentAsync filters out revoked (IsActive=false) assignments");
    }

    [Fact]
    public async Task GetAssignmentsAsync_ReturnsOnlyActiveAssignments()
    {
        await SeedApplicationAsync();
        var active  = NewAssignment("active-member@test");
        var revoked = NewAssignment("revoked-member@test");

        await _repo.CreateAssignmentAsync(active);
        await _repo.CreateAssignmentAsync(revoked);
        await _repo.RevokeAssignmentAsync(_orgId, _appId, "revoked-member@test");

        var results = (await _repo.GetAssignmentsAsync(_orgId, _appId)).ToList();

        results.Should().Contain(a => a.UserPrincipalId == "active-member@test");
        results.Should().NotContain(a => a.UserPrincipalId == "revoked-member@test",
            "revoked assignments must be excluded from GetAssignmentsAsync");
    }

    [Fact]
    public async Task CreateAssignmentAsync_ThrowsOnDuplicate_UniqueIndex()
    {
        await SeedApplicationAsync();
        var upn = $"dup-user-{Guid.NewGuid():N}@test";
        await _repo.CreateAssignmentAsync(NewAssignment(upn));

        // A second assignment for the same (org, app, user) triplet must fail.
        Func<Task> act = () => _repo.CreateAssignmentAsync(NewAssignment(upn));

        await act.Should().ThrowAsync<MongoWriteException>(
            "the unique index idx_userassignment_unique must reject duplicate (org, app, user) triplets");
    }
}
