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
using Xunit.Abstractions;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

public class OrganisationUpdatedHandlerIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly UserManagementPostgreSqlFixture _fixture;

    public OrganisationUpdatedHandlerIntegrationTests(
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

    [Fact]
    public async Task Handle_WithExistingOrg_UpdatesName()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        CreateOrganisation(orgGuid, "Old Name");
        var @event = new OrganisationUpdated
        {
            Id = orgGuid.ToString(),
            Name = "New Name"
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new OrganisationUpdatedHandler(syncRepo, unitOfWork, NullLogger<OrganisationUpdatedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.SingleOrDefault(o => o.CdpOrganisationGuid == orgGuid);
        org.Should().NotBeNull();
        org!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_CreatesOrg()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var @event = new OrganisationUpdated
        {
            Id = orgGuid.ToString(),
            Name = "Brand New Org"
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new OrganisationUpdatedHandler(syncRepo, unitOfWork, NullLogger<OrganisationUpdatedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.SingleOrDefault(o => o.CdpOrganisationGuid == orgGuid);
        org.Should().NotBeNull();
        org!.Name.Should().Be("Brand New Org");
    }

    [Fact]
    public async Task Handle_IsIdempotent_WhenNameUnchanged()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        CreateOrganisation(orgGuid, "Same Name");
        var @event = new OrganisationUpdated
        {
            Id = orgGuid.ToString(),
            Name = "Same Name"
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new OrganisationUpdatedHandler(syncRepo, unitOfWork, NullLogger<OrganisationUpdatedHandler>.Instance);
            await handler.Handle(@event);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var handler =
                new OrganisationUpdatedHandler(syncRepo, unitOfWork, NullLogger<OrganisationUpdatedHandler>.Instance);
            await handler.Handle(@event);
        }

        using var db = _fixture.UserManagementContext();
        var org = db.Organisations.SingleOrDefault(o => o.CdpOrganisationGuid == orgGuid);
        org.Should().NotBeNull();
        org!.Name.Should().Be("Same Name");
        db.Organisations.Count(o => o.CdpOrganisationGuid == orgGuid).Should().Be(1);
    }
}