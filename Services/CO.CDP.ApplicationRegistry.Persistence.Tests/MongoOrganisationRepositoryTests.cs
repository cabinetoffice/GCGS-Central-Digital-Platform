using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories.MongoDB;
using FluentAssertions;
using MongoDB.Driver;

namespace CO.CDP.ApplicationRegistry.Persistence.Tests;

[Collection("MongoDB")]
public class MongoOrganisationRepositoryTests
{
    private readonly MongoOrganisationRepository _repo;
    private readonly MongoDB.MongoAppRegistryDatabase _db;

    public MongoOrganisationRepositoryTests(MongoDbFixture fixture)
    {
        // Isolated database — different name from the Application tests.
        _db   = fixture.CreateDatabase("app_tests_organisation");
        _repo = new MongoOrganisationRepository(_db, fixture.StubAudit(), fixture.StubCurrentUser());
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Organisation NewOrg(string? slug = null, string? name = null) => new()
    {
        Name     = name ?? $"Org-{Guid.NewGuid():N}",
        Slug     = slug ?? $"org-{Guid.NewGuid():N}",
        Type     = "Government",
        IsActive = true
    };

    private static UserOrganisationMembership NewMembership(Guid orgId, string? upn = null) => new()
    {
        OrganisationId   = orgId,
        UserPrincipalId  = upn ?? $"user-{Guid.NewGuid():N}",
        OrganisationRole = "Member",
        IsActive         = true
    };

    /// <summary>
    /// Creates a minimal Application document directly in the shared applications
    /// collection (within the organisation test database) so EnableApplicationAsync
    /// can reference a real application ID.
    /// </summary>
    private async Task<Application> SeedApplicationAsync(Guid? id = null)
    {
        var app = new Application
        {
            Id       = id ?? Guid.NewGuid(),
            Name     = $"App-{Guid.NewGuid():N}",
            ClientId = $"client-{Guid.NewGuid():N}",
            IsActive = true
        };
        await _db.Applications.InsertOneAsync(app);
        return app;
    }

    // ── Tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsOrganisation()
    {
        var org = NewOrg(slug: "test-org-create");

        var created = await _repo.CreateAsync(org);

        var fetched = await _repo.GetByIdAsync(created.Id);
        fetched.Should().NotBeNull();
        fetched!.Slug.Should().Be("test-org-create");
        fetched.Name.Should().Be(org.Name);
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsCorrectOrganisation()
    {
        var slug = $"slug-{Guid.NewGuid():N}";
        var org  = await _repo.CreateAsync(NewOrg(slug: slug));

        var fetched = await _repo.GetBySlugAsync(slug);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(org.Id);
    }

    [Fact]
    public async Task GetMemberAsync_ReturnsActiveMember()
    {
        var org    = await _repo.CreateAsync(NewOrg());
        var member = NewMembership(org.Id, "active-user@test");
        await _repo.AddMemberAsync(member);

        var found = await _repo.GetMemberAsync(org.Id, "active-user@test");

        found.Should().NotBeNull();
        found!.UserPrincipalId.Should().Be("active-user@test");
        found.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetMemberAsync_ReturnsNull_WhenMemberIsInactive()
    {
        var org    = await _repo.CreateAsync(NewOrg());
        var member = NewMembership(org.Id, "inactive-user@test");
        await _repo.AddMemberAsync(member);

        // Deactivate via UpdateMember.
        member.IsActive = false;
        await _repo.UpdateMemberAsync(member);

        var found = await _repo.GetMemberAsync(org.Id, "inactive-user@test");
        found.Should().BeNull("GetMemberAsync must filter out inactive members");
    }

    [Fact]
    public async Task EnableApplicationAsync_AddsApplicationToOrg()
    {
        var org = await _repo.CreateAsync(NewOrg());
        var app = await SeedApplicationAsync();

        var oa = new OrganisationApplication
        {
            OrganisationId = org.Id,
            ApplicationId  = app.Id,
            EnabledBy      = "test-user",
            IsEnabled      = true
        };
        await _repo.EnableApplicationAsync(oa);

        var apps = (await _repo.GetOrganisationApplicationsAsync(org.Id)).ToList();
        apps.Should().ContainSingle(a => a.ApplicationId == app.Id);
    }

    [Fact]
    public async Task DisableApplicationAsync_SetsIsEnabledFalse()
    {
        var org = await _repo.CreateAsync(NewOrg());
        var app = await SeedApplicationAsync();

        var oa = new OrganisationApplication
        {
            OrganisationId = org.Id,
            ApplicationId  = app.Id,
            EnabledBy      = "test-user",
            IsEnabled      = true
        };
        await _repo.EnableApplicationAsync(oa);
        await _repo.DisableApplicationAsync(org.Id, app.Id);

        // Raw document must show IsEnabled=false, DisabledAt set, DisabledBy set.
        var raw = await _db.Organisations
            .Find(o => o.Id == org.Id)
            .FirstOrDefaultAsync();

        raw.Should().NotBeNull();
        var rawApp = raw!.Applications.FirstOrDefault(a => a.ApplicationId == app.Id);
        rawApp.Should().NotBeNull();
        rawApp!.IsEnabled.Should().BeFalse();
        rawApp.DisabledAt.Should().NotBeNull();
        rawApp.DisabledBy.Should().Be("test-user");
    }

    [Fact]
    public async Task GetOrganisationApplicationsAsync_FiltersDisabledApplications()
    {
        var org  = await _repo.CreateAsync(NewOrg());
        var app1 = await SeedApplicationAsync();
        var app2 = await SeedApplicationAsync();

        await _repo.EnableApplicationAsync(new OrganisationApplication
        {
            OrganisationId = org.Id, ApplicationId = app1.Id,
            EnabledBy = "test-user", IsEnabled = true
        });
        await _repo.EnableApplicationAsync(new OrganisationApplication
        {
            OrganisationId = org.Id, ApplicationId = app2.Id,
            EnabledBy = "test-user", IsEnabled = true
        });
        await _repo.DisableApplicationAsync(org.Id, app2.Id);

        var active = (await _repo.GetOrganisationApplicationsAsync(org.Id)).ToList();

        active.Should().ContainSingle(a => a.ApplicationId == app1.Id,
            "only enabled applications should be returned");
        active.Should().NotContain(a => a.ApplicationId == app2.Id,
            "disabled applications must be excluded");
    }
}
