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
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

public class PersonInviteClaimedHandlerIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly UserManagementPostgreSqlFixture _fixture;
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();

    public PersonInviteClaimedHandlerIntegrationTests(
        UserManagementPostgreSqlFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;

        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole>());

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

    private UmOrganisation CreateOrganisationWithAppsEnabled(Guid cdpGuid, string name = "Test Org")
    {
        using var db = _fixture.UserManagementContext();
        var org = new UmOrganisation
        {
            CdpOrganisationGuid = cdpGuid,
            Name = name,
            Slug = Guid.NewGuid().ToString(),
            IsActive = true,
            CreatedBy = "test"
        };
        db.Organisations.Add(org);
        db.SaveChanges();
        return org;
    }

    private async Task EnableApplicationsForOrg(Guid orgGuid)
    {
        using var scope = _factory.Services.CreateScope();
        var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await syncRepo.EnsureActiveApplicationsEnabledAsync(orgGuid);
        await unitOfWork.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WithAdminScope_CreatesAdminMembership()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        CreateOrganisationWithAppsEnabled(orgGuid);
        await EnableApplicationsForOrg(orgGuid);
        var @event = new PersonInviteClaimed
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            UserPrincipalId = "user|invite-admin-test",
            Scopes = ["ADMIN"],
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonInviteClaimedHandler(syncRepo, unitOfWork, NullLogger<PersonInviteClaimedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        var membership = db.UserOrganisationMemberships
            .SingleOrDefault(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        membership.Should().NotBeNull();
        membership!.OrganisationRoleId.Should().Be((int)OrganisationRole.Admin);
    }

    [Fact]
    public async Task Handle_WithNoAdminScope_CreatesMemberMembership()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        CreateOrganisationWithAppsEnabled(orgGuid);
        await EnableApplicationsForOrg(orgGuid);
        var @event = new PersonInviteClaimed
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            UserPrincipalId = "user|invite-member-test",
            Scopes = ["VIEWER"],
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonInviteClaimedHandler(syncRepo, unitOfWork, NullLogger<PersonInviteClaimedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        var membership = db.UserOrganisationMemberships
            .SingleOrDefault(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        membership.Should().NotBeNull();
        membership!.OrganisationRoleId.Should().Be((int)OrganisationRole.Member);
    }

    [Fact]
    public async Task Handle_WithTendererRoleAndFat_AssignsAppRole()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(orgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole> { CorePartyRole.Tenderer });
        CreateOrganisationWithAppsEnabled(orgGuid);
        await EnableApplicationsForOrg(orgGuid);
        var @event = new PersonInviteClaimed
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            UserPrincipalId = "user|invite-fat-tenderer-test",
            Scopes = ["ADMIN"],
        };
        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonInviteClaimedHandler(syncRepo, unitOfWork, NullLogger<PersonInviteClaimedHandler>.Instance);
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
    public async Task Handle_IsIdempotent_WhenCalledTwice()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        CreateOrganisationWithAppsEnabled(orgGuid);
        await EnableApplicationsForOrg(orgGuid);
        var @event = new PersonInviteClaimed
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            UserPrincipalId = "user|invite-idempotent-test",
            Scopes = ["ADMIN"],
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonInviteClaimedHandler(syncRepo, unitOfWork, NullLogger<PersonInviteClaimedHandler>.Instance);
            await handler.Handle(@event);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonInviteClaimedHandler(syncRepo, unitOfWork, NullLogger<PersonInviteClaimedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.Single(o => o.CdpOrganisationGuid == orgGuid);
        db.UserOrganisationMemberships
            .Count(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id)
            .Should().Be(1);
    }
}