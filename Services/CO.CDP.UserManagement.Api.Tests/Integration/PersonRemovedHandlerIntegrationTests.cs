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
using Xunit.Abstractions;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using UserOrganisationMembership = CO.CDP.UserManagement.Core.Entities.UserOrganisationMembership;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

public class PersonRemovedHandlerIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly UserManagementPostgreSqlFixture _fixture;

    public PersonRemovedHandlerIntegrationTests(
        UserManagementPostgreSqlFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
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

    private void CreateMembership(int orgId,
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
    }

    [Fact]
    public async Task Handle_WithExistingMember_DeactivatesMembership()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var org = CreateOrganisation(orgGuid);
        CreateMembership(org.Id, personGuid, "user|remove-test");
        var @event = new PersonRemovedFromOrganisation
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString()
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new PersonRemovedHandler(syncRepo, unitOfWork, NullLogger<PersonRemovedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var membership = db.UserOrganisationMemberships
            .IgnoreQueryFilters()
            .SingleOrDefault(m => m.CdpPersonId == personGuid && m.OrganisationId == org.Id);
        membership.Should().NotBeNull();
        membership!.IsActive.Should().BeFalse();
        membership.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenPersonNotMember_IsNoOp()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        CreateOrganisation(orgGuid);
        var @event = new PersonRemovedFromOrganisation
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString()
        };

        var act = async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new PersonRemovedHandler(syncRepo, unitOfWork, NullLogger<PersonRemovedHandler>.Instance);
            await handler.Handle(@event);
        };

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_IsNoOp()
    {
        ClearDatabase();
        var @event = new PersonRemovedFromOrganisation
        {
            OrganisationId = Guid.NewGuid().ToString(),
            PersonId = Guid.NewGuid().ToString()
        };

        var act = async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler = new PersonRemovedHandler(syncRepo, unitOfWork, NullLogger<PersonRemovedHandler>.Instance);
            await handler.Handle(@event);
        };

        await act.Should().NotThrowAsync();
    }
}