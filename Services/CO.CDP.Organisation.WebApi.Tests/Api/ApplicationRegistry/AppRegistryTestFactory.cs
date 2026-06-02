using CO.CDP.ApplicationRegistry.Persistence.MongoDB;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.MQ;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.TestKit.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Moq;
using System.Security.Claims;

namespace CO.CDP.Organisation.WebApi.Tests.Api.ApplicationRegistry;

/// <summary>
/// Factory helpers for AppRegistry endpoint tests.
/// AppRegistry uses its own authorization policies (PlatformAdmin, OrgAdmin, OrgMember)
/// backed by <c>platform_role=admin</c> and <c>org:{orgId}:role</c> JWT claims,
/// which differ from the main Organisation API policies.
/// </summary>
internal static class AppRegistryTestFactory
{
    /// <summary>Creates a client authenticated as a Platform Admin (platform_role=admin).</summary>
    public static HttpClient PlatformAdmin(Action<IServiceCollection>? extraServices = null)
        => Create([new Claim("platform_role", "admin")], extraServices);

    /// <summary>Creates a client authenticated as an Org Admin for the given org.</summary>
    public static HttpClient OrgAdmin(Guid orgId, Action<IServiceCollection>? extraServices = null)
        => Create([new Claim($"org:{orgId}:role", "Admin")], extraServices);

    /// <summary>Creates a client authenticated as an Org Member for the given org.</summary>
    public static HttpClient OrgMember(Guid orgId, Action<IServiceCollection>? extraServices = null)
        => Create([new Claim($"org:{orgId}:role", "Member")], extraServices);

    /// <summary>Creates a client with no claims matching any AppRegistry policy (Forbidden).</summary>
    public static HttpClient Unauthorized(Action<IServiceCollection>? extraServices = null)
        => Create([], extraServices);

    // ── Core factory ───────────────────────────────────────────────────────

    private static HttpClient Create(IEnumerable<Claim> authClaims, Action<IServiceCollection>? extraServices)
    {
        var factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace MongoDB singleton with a no-op mock so tests start without a
                // live MongoDB connection. EnsureIndexes() is now called fire-and-forget
                // from ApplicationStarted, so it fails silently in tests.
                var mockMongoDb = new Mock<IMongoDatabase>();
                SetupMockCollections(mockMongoDb);
                services.AddSingleton<IMongoDatabase>(mockMongoDb.Object);

                // Replace IPublisher (required by the existing TestWebApplicationFactory)
                services.AddScoped(_ => new Mock<IPublisher>().Object);

                // Use an in-memory DbContext for OrganisationInformationContext so each
                // factory creation does NOT trigger a new EF Core service provider.
                // Without this, creating >20 factories in one test run hits the EF Core limit.
                services.ConfigureInMemoryDbContext<OrganisationInformationContext>();

                // Apply AppRegistry-specific authorization claims
                services.AddTransient<IPolicyEvaluator>(sp =>
                    new AppRegistryPolicyEvaluator(
                        ActivatorUtilities.CreateInstance<PolicyEvaluator>(sp),
                        authClaims.ToList()));

                // Extra per-test service overrides (repository mocks, use-case mocks, etc.)
                extraServices?.Invoke(services);
            });
        });

        return factory.CreateClient();
    }

    /// <summary>
    /// Sets up mock collections on the IMongoDatabase mock so that
    /// <see cref="MongoAppRegistryDatabase"/> can be constructed without
    /// triggering actual MongoDB operations.
    /// </summary>
    private static void SetupMockCollections(Mock<IMongoDatabase> mockDb)
    {
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.Application>(mockDb);
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.Organisation>(mockDb);
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.UserApplicationAssignment>(mockDb);
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.FeatureFlag>(mockDb);
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.AuditLog>(mockDb);
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.AccessControlEntry>(mockDb);
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.ReportCategory>(mockDb);
        SetupCollection<CO.CDP.ApplicationRegistry.Persistence.Entities.ReportCategoryAssignment>(mockDb);
    }

    private static void SetupCollection<T>(Mock<IMongoDatabase> mockDb)
    {
        var mockCollection = new Mock<IMongoCollection<T>>();
        var mockIndexManager = new Mock<IMongoIndexManager<T>>();

        mockIndexManager
            .Setup(m => m.CreateMany(
                It.IsAny<IEnumerable<CreateIndexModel<T>>>(),
                It.IsAny<CancellationToken>()))
            .Returns([]);

        mockCollection.Setup(c => c.Indexes).Returns(mockIndexManager.Object);

        mockDb
            .Setup(db => db.GetCollection<T>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings?>()))
            .Returns(mockCollection.Object);
    }
}

/// <summary>
/// IPolicyEvaluator that injects AppRegistry-specific claims (platform_role, org:{id}:role)
/// into the HttpContext user identity for authorization policy evaluation.
/// </summary>
internal class AppRegistryPolicyEvaluator(PolicyEvaluator inner, IReadOnlyList<Claim> claims)
    : IPolicyEvaluator
{
    private const string Scheme = "AppRegistryTest";

    public Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> AuthenticateAsync(
        AuthorizationPolicy policy, HttpContext context)
    {
        var identity = new ClaimsIdentity(Scheme);
        identity.AddClaim(new Claim("channel", "one-login"));
        identity.AddClaim(new Claim("sub", "urn:test:user"));
        identity.AddClaims(claims);

        var principal = new ClaimsPrincipal(identity);
        var ticket    = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(
            principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties(), Scheme);

        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        Microsoft.AspNetCore.Authentication.AuthenticateResult authResult,
        HttpContext context,
        object? resource)
        => inner.AuthorizeAsync(policy, authResult, context, resource);
}
