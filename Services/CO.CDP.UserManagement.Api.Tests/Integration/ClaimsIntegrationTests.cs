using System.Net;
using System.Net.Http.Json;
using CO.CDP.TestKit.Mvc;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;

using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using UmApplication = CO.CDP.UserManagement.Core.Entities.Application;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

public class ClaimsIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly UserManagementPostgreSqlFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public ClaimsIntegrationTests(UserManagementPostgreSqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
    }

    private HttpClient CreateClient(bool featureEnabled = true)
    {
        var factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(_testOutputHelper);

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { Shared.FeatureFlags.FeatureFlags.ClaimsApiEnabled, featureEnabled.ToString() },
                    { "Aws:ElastiCache:Hostname", _fixture.RedisHost },
                    { "Aws:ElastiCache:Port", _fixture.RedisPort }
                });
            });

            builder.ConfigureServices((_, services) =>
            {
                services.RemoveAll<UserManagementDbContext>();
                services.RemoveAll<DbContextOptions<UserManagementDbContext>>();
                services.AddDbContext<UserManagementDbContext>((sp, options) =>
                    options.UseNpgsql(_fixture.ConnectionString,
                            npgsqlOptions => npgsqlOptions
                                .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));
            });
        });

        return factory.CreateClient();
    }

    [Fact]
    public async Task GetUserClaims_WhenFeatureFlagDisabled_ReturnsNotFound()
    {
        var client = CreateClient(featureEnabled: false);
        var response = await client.GetAsync("/api/claims/users/user-1");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserClaims_WhenFeatureFlagEnabled_ReturnsOk()
    {
        var client = CreateClient(featureEnabled: true);
        var userPrincipalId = "user-claims-test";
        var cdpOrgGuid = Guid.NewGuid();
        var appGuid = Guid.NewGuid();

        using (var context = _fixture.UserManagementContext())
        {
            context.Database.EnsureCreated();

            // Clear existing data to avoid conflicts if test runs multiple times on dirty DB
            context.UserApplicationAssignments.RemoveRange(context.UserApplicationAssignments);
            context.UserOrganisationMemberships.RemoveRange(context.UserOrganisationMemberships);
            context.OrganisationApplications.RemoveRange(context.OrganisationApplications);
            context.Applications.RemoveRange(context.Applications);
            context.Organisations.RemoveRange(context.Organisations);
            await context.SaveChangesAsync();

            var app = new UmApplication
            {
                Name = "Test App",
                Guid = appGuid,
                ClientId = "client-id",
                // RootUrl removed
            };
            context.Applications.Add(app);

            var org = new UmOrganisation
            {
                Name = "Test Org",
                CdpOrganisationGuid = cdpOrgGuid,
                Slug = "test-org",
                IsActive = true
            };
            context.Organisations.Add(org);

            var orgApp = new OrganisationApplication
            {
                Organisation = org,
                Application = app,
                IsActive = true
            };
            context.OrganisationApplications.Add(orgApp);

            var membership = new UserOrganisationMembership
            {
                UserPrincipalId = userPrincipalId,
                Organisation = org,
                OrganisationRole = Shared.Enums.OrganisationRole.Admin,
                IsActive = true,
                JoinedAt = DateTimeOffset.UtcNow
            };
            context.UserOrganisationMemberships.Add(membership);

            var assignment = new UserApplicationAssignment
            {
                UserOrganisationMembership = membership,
                OrganisationApplication = orgApp,
                IsActive = true,
                AssignedAt = DateTimeOffset.UtcNow
            };
            context.UserApplicationAssignments.Add(assignment);

            await context.SaveChangesAsync();
        }

        var response = await client.GetAsync($"/api/claims/users/{userPrincipalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var claims = await response.Content.ReadFromJsonAsync<UserClaims>();

        claims.Should().NotBeNull();
        claims!.UserPrincipalId.Should().Be(userPrincipalId);
        claims.Organisations.Should().HaveCount(1);

        var orgClaim = claims.Organisations.First();
        orgClaim.OrganisationId.Should().Be(cdpOrgGuid);
        orgClaim.OrganisationName.Should().Be("Test Org");
        orgClaim.OrganisationRole.Should().Be("Admin");

        orgClaim.Applications.Should().HaveCount(1);
        var appClaim = orgClaim.Applications.First();
        appClaim.ApplicationId.Should().Be(appGuid);
        appClaim.ApplicationName.Should().Be("Test App");
        appClaim.ClientId.Should().Be("client-id");
    }
}