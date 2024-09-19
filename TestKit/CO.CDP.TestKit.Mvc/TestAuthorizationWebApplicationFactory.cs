using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Security.Claims;

namespace CO.CDP.TestKit.Mvc;

public class TestAuthorizationWebApplicationFactory<TProgram>(
        Claim[] claimFeed,
        Guid? organisationId = null,
        string[]? assignedOrganisationScopes = null,
        Action<IServiceCollection>? serviceCollection = null)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (serviceCollection != null) builder.ConfigureServices(serviceCollection);

        builder.ConfigureServices(services =>
        {
            services.AddTransient<IPolicyEvaluator>(sp => new AuthorizationPolicyEvaluator(
                ActivatorUtilities.CreateInstance<PolicyEvaluator>(sp), claimFeed, assignedOrganisationScopes));

            if (assignedOrganisationScopes?.Length > 0)
            {
                Mock<ITenantRepository> mockDatabaseTenantRepo = new();
                mockDatabaseTenantRepo.Setup(r => r.LookupTenant("urn:fake_user"))
                    .ReturnsAsync(new TenantLookup
                    {
                        User = new TenantLookup.PersonUser { Name = "Test", Email = "test@test", Urn = "urn:fake_user" },
                        Tenants = [new TenantLookup.Tenant { Id = Guid.NewGuid(), Name = "Ten",
                            Organisations = [new TenantLookup.Organisation { Id = organisationId ?? Guid.NewGuid(), Name = "org", Roles = [], Scopes = [.. assignedOrganisationScopes] }] }]
                    });

                services.AddTransient(sc => mockDatabaseTenantRepo.Object);
            }
        });

        return base.CreateHost(builder);
    }
}

public class AuthorizationPolicyEvaluator(PolicyEvaluator innerEvaluator, Claim[] claimFeed, string[]? assignedOrganisationScopes) : IPolicyEvaluator
{
    const string JwtBearerOrApiKeyScheme = "JwtBearer_Or_ApiKey";

    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var claimsIdentity = new ClaimsIdentity(JwtBearerOrApiKeyScheme);
        if (claimFeed.Length > 0) claimsIdentity.AddClaims(claimFeed);
        if (assignedOrganisationScopes?.Length > 0) claimsIdentity.AddClaim(new Claim("sub", "urn:fake_user"));

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties(), JwtBearerOrApiKeyScheme)));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return innerEvaluator.AuthorizeAsync(policy, authenticationResult, context, resource);
    }
}