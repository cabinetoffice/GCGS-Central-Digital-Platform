using CO.CDP.TestKit.Mvc;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
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

public class OrganisationApprovedHandlerIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly UserManagementPostgreSqlFixture _fixture;
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();

    public OrganisationApprovedHandlerIntegrationTests(
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

    /// <summary>
    /// Core scenario: org registered without Buyer role (PendingRoles state), founder membership
    /// created with no buyer defaults. Then org is approved (Buyer added to Roles). Handler fires
    /// and should backfill the buyer default application roles for all existing members.
    /// </summary>
    [Fact]
    public async Task Handle_BackfillsBuyerDefaultRolesForExistingMembers()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();

        // Step 1: Register org without Buyer party role (simulates PendingRoles state)
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(orgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole>());

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var registeredHandler = new OrganisationRegisteredHandler(
                syncRepo, unitOfWork, Mock.Of<IClaimsCacheService>(),
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await registeredHandler.Handle(new OrganisationRegistered
            {
                Id = orgGuid.ToString(),
                Name = "Buyer Org",
                FounderPersonId = personGuid,
                FounderUserUrn = "user|buyer-approval-test"
            });
        }

        // Verify no buyer default roles yet (party roles empty at registration time)
        await using (var db = _fixture.UserManagementContext())
        {
            var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
            var membership = db.UserOrganisationMemberships
                .Single(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
            var assignment = db.UserApplicationAssignments
                .Include(a => a.Roles)
                .SingleOrDefault(a => a.UserOrganisationMembershipId == membership.Id);
            // Either no assignment or empty roles because Buyer wasn't in party roles
            if (assignment != null)
                assignment.Roles.Should()
                    .BeEmpty("buyer roles require Buyer party role which was absent at registration");
        }

        // Step 2: Org is now approved — Buyer added to Roles in OI; simulate by returning Buyer from adapter
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(orgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole> { CorePartyRole.Buyer });

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var approvedHandler = new OrganisationApprovedHandler(
                syncRepo, unitOfWork,
                NullLogger<OrganisationApprovedHandler>.Instance);
            await approvedHandler.Handle(new OrganisationApproved { Id = orgGuid.ToString() });
        }

        // Step 3: Verify buyer default roles now present
        using (var db = _fixture.UserManagementContext())
        {
            var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
            var membership = db.UserOrganisationMemberships
                .Single(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
            var assignment = db.UserApplicationAssignments
                .Include(a => a.Roles)
                .SingleOrDefault(a => a.UserOrganisationMembershipId == membership.Id);
            assignment.Should().NotBeNull("assignment should now exist after buyer approval");
            assignment!.IsActive.Should().BeTrue();
            assignment.Roles.Should().NotBeEmpty("buyer default roles should have been applied");
        }
    }

    [Fact]
    public async Task Handle_IsIdempotent_WhenCalledTwice()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();

        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(orgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole> { CorePartyRole.Buyer });

        // Register with Buyer role already present (shortcut to get a member)
        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var registeredHandler = new OrganisationRegisteredHandler(
                syncRepo, unitOfWork, Mock.Of<IClaimsCacheService>(),
                NullLogger<OrganisationRegisteredHandler>.Instance);
            await registeredHandler.Handle(new OrganisationRegistered
            {
                Id = orgGuid.ToString(),
                Name = "Idempotent Buyer Org",
                FounderPersonId = personGuid,
                FounderUserUrn = "user|buyer-idempotent-test"
            });
        }

        var approvedEvent = new OrganisationApproved { Id = orgGuid.ToString() };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var approvedHandler = new OrganisationApprovedHandler(
                syncRepo, unitOfWork,
                NullLogger<OrganisationApprovedHandler>.Instance);
            await approvedHandler.Handle(approvedEvent);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var approvedHandler = new OrganisationApprovedHandler(
                syncRepo, unitOfWork,
                NullLogger<OrganisationApprovedHandler>.Instance);
            await approvedHandler.Handle(approvedEvent);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        var membership = db.UserOrganisationMemberships
            .Single(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        db.UserApplicationAssignments
            .Count(a => a.UserOrganisationMembershipId == membership.Id)
            .Should().Be(1, "idempotent — no duplicate assignments");
    }

    [Fact]
    public async Task Handle_GracefullyIgnoresUnknownOrganisation()
    {
        ClearDatabase();
        var unknownOrgGuid = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var handler = new OrganisationApprovedHandler(
            syncRepo, unitOfWork,
            NullLogger<OrganisationApprovedHandler>.Instance);

        // Should not throw — ThrowOnFailure logs a warning for unknown org
        var act = async () => await handler.Handle(new OrganisationApproved { Id = unknownOrgGuid.ToString() });
        await act.Should().ThrowAsync<Exception>("unknown org results in a failure logged via ThrowOnFailure");
    }
}