using CO.CDP.TestKit.Mvc;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit.Abstractions;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

public class OrganisationRegisteredHandlerIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly UserManagementPostgreSqlFixture _fixture;
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();

    public OrganisationRegisteredHandlerIntegrationTests(
        UserManagementPostgreSqlFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;

        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole>());

        _factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);
            builder.ConfigureServices((_, services) =>
            {
                services.PostConfigure<RedisCacheOptions>(o =>
                {
                    o.Configuration = $"{fixture.RedisHost}:{fixture.RedisPort}";
                    o.InstanceName = "UserManagement_";
                });
                services.RemoveAll<UserManagementDbContext>();
                services.RemoveAll<DbContextOptions<UserManagementDbContext>>();
                services.AddDbContext<UserManagementDbContext>((sp, opts) =>
                    opts.UseNpgsql(fixture.ConnectionString, npgsql => npgsql
                            .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

                services.RemoveAll<IOrganisationApiAdapter>();
                services.AddScoped(_ => _organisationApiAdapter.Object);
            });
        });
    }

    private void ClearDatabase()
    {
        using var db = _fixture.UserManagementContext();
        db.UserApplicationAssignments.RemoveRange(db.UserApplicationAssignments.IgnoreQueryFilters().ToList());
        db.UserOrganisationMemberships.RemoveRange(db.UserOrganisationMemberships.IgnoreQueryFilters().ToList());
        db.OrganisationApplications.RemoveRange(db.OrganisationApplications.IgnoreQueryFilters().ToList());
        db.Organisations.RemoveRange(db.Organisations.IgnoreQueryFilters().ToList());
        db.SaveChanges();
    }

    [Fact]
    public async Task Handle_CreatesUmOrganisation()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var @event = new OrganisationRegistered
        {
            Id = orgGuid.ToString(),
            Name = "Test Organisation",
            Roles = [],
            Type = 0
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new OrganisationRegisteredHandler(syncRepo, unitOfWork,
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.SingleOrDefault(o => o.CdpOrganisationGuid == orgGuid);
        org.Should().NotBeNull();
        org!.Name.Should().Be("Test Organisation");
    }

    [Fact]
    public async Task Handle_IsIdempotent_WhenCalledTwice()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var @event = new OrganisationRegistered
        {
            Id = orgGuid.ToString(),
            Name = "Idempotent Org",
            Roles = [],
            Type = 0
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new OrganisationRegisteredHandler(syncRepo, unitOfWork,
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await handler.Handle(@event);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new OrganisationRegisteredHandler(syncRepo, unitOfWork,
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        db.Organisations.Count(o => o.CdpOrganisationGuid == orgGuid).Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithFounder_CreatesOwnerMembership()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var @event = new OrganisationRegistered
        {
            Id = orgGuid.ToString(),
            Name = "Founder Org",
            Roles = [],
            Type = 0,
            FounderPersonId = personGuid,
            FounderUserUrn = "user|founder-test"
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new OrganisationRegisteredHandler(syncRepo, unitOfWork,
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        var membership = db.UserOrganisationMemberships
            .SingleOrDefault(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        membership.Should().NotBeNull();
        membership!.OrganisationRoleId.Should().Be((int)OrganisationRole.Owner);
    }

    [Fact]
    public async Task Handle_WithFounderAndTendererRole_AssignsFatAppRole()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var @event = new OrganisationRegistered
        {
            Id = orgGuid.ToString(),
            Name = "FaT Tenderer Org",
            Roles = ["tenderer"],
            Type = 0,
            FounderPersonId = personGuid,
            FounderUserUrn = "user|fat-tenderer-test"
        };

        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(orgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole> { CorePartyRole.Tenderer });

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new OrganisationRegisteredHandler(syncRepo, unitOfWork,
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        var membership = db.UserOrganisationMemberships
            .Single(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        var assignment = db.UserApplicationAssignments
            .Include(a => a.Roles)
            .SingleOrDefault(a => a.UserOrganisationMembershipId == membership.Id);
        assignment.Should().NotBeNull();
        assignment!.Roles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithoutFounder_DoesNotCreateMembership()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var @event = new OrganisationRegistered
        {
            Id = orgGuid.ToString(),
            Name = "No Founder Org",
            Roles = [],
            Type = 0
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new OrganisationRegisteredHandler(syncRepo, unitOfWork,
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        db.UserOrganisationMemberships.Any(m => m.OrganisationId == org.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_EnablesActiveApplications()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var @event = new OrganisationRegistered
        {
            Id = orgGuid.ToString(),
            Name = "App Enabled Org",
            Roles = [],
            Type = 0
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new OrganisationRegisteredHandler(syncRepo, unitOfWork,
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        db.OrganisationApplications.Any(oa => oa.OrganisationId == org.Id && oa.IsActive).Should().BeTrue();
    }
}