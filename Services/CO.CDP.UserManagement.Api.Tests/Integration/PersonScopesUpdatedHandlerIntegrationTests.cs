using CO.CDP.TestKit.Mvc;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Infrastructure.Events;
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
using UserOrganisationMembership = CO.CDP.UserManagement.Core.Entities.UserOrganisationMembership;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

public class PersonScopesUpdatedHandlerIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly UserManagementPostgreSqlFixture _fixture;
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();

    public PersonScopesUpdatedHandlerIntegrationTests(
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

    private UmOrganisation CreateOrganisation(Guid cdpGuid, string name = "Test Org")
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

    private UserOrganisationMembership CreateMembership(
        int orgId,
        Guid personGuid,
        string userPrincipalId = "user|123",
        int roleId = (int)OrganisationRole.Member)
    {
        using var db = _fixture.UserManagementContext();
        var membership = new UserOrganisationMembership
        {
            CdpPersonId = personGuid,
            UserPrincipalId = userPrincipalId,
            OrganisationId = orgId,
            OrganisationRoleId = roleId,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
        db.UserOrganisationMemberships.Add(membership);
        db.SaveChanges();
        return membership;
    }

    [Fact]
    public async Task Handle_WithAdminScope_SetsAdminRole()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var org = CreateOrganisation(orgGuid);
        CreateMembership(org.Id, personGuid, "user|scopes-admin-test", (int)OrganisationRole.Member);
        var @event = new PersonScopesUpdated
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            Scopes = ["ADMIN"],
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonScopesUpdatedHandler(syncRepo, unitOfWork, NullLogger<PersonScopesUpdatedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var membership = db.UserOrganisationMemberships
            .SingleOrDefault(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        membership.Should().NotBeNull();
        membership!.OrganisationRoleId.Should().Be((int)OrganisationRole.Admin);
    }

    [Fact]
    public async Task Handle_WithoutAdminScope_SetsMemberRole()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var org = CreateOrganisation(orgGuid);
        CreateMembership(org.Id, personGuid, "user|scopes-member-test", (int)OrganisationRole.Admin);
        var @event = new PersonScopesUpdated
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            Scopes = ["VIEWER"],
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonScopesUpdatedHandler(syncRepo, unitOfWork, NullLogger<PersonScopesUpdatedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var membership = db.UserOrganisationMemberships
            .SingleOrDefault(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        membership.Should().NotBeNull();
        membership!.OrganisationRoleId.Should().Be((int)OrganisationRole.Member);
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_IsNoOp()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        CreateOrganisation(orgGuid);
        var @event = new PersonScopesUpdated
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = Guid.NewGuid().ToString(),
            Scopes = ["ADMIN"],
        };

        var act = async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonScopesUpdatedHandler(syncRepo, unitOfWork, NullLogger<PersonScopesUpdatedHandler>.Instance);
            await handler.Handle(@event);
        };

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithTendererRole_RecalculatesFatAppRoles()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(orgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole> { CorePartyRole.Tenderer });
        var org = CreateOrganisation(orgGuid);
        CreateMembership(org.Id, personGuid, "user|fat-scopes-test", (int)OrganisationRole.Member);
        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await syncRepo.EnsureActiveApplicationsEnabledAsync(orgGuid);
            await unitOfWork.SaveChangesAsync();
        }

        var @event = new PersonScopesUpdated
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            Scopes = ["ADMIN"],
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new PersonScopesUpdatedHandler(syncRepo, unitOfWork, NullLogger<PersonScopesUpdatedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var membership = db.UserOrganisationMemberships
            .Single(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        var assignment = db.UserApplicationAssignments
            .Include(a => a.Roles)
            .SingleOrDefault(a => a.UserOrganisationMembershipId == membership.Id);
        assignment.Should().NotBeNull();
        assignment!.Roles.Should().NotBeEmpty();
    }
}