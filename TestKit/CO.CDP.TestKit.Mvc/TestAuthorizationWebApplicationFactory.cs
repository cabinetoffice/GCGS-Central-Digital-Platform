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
        string channel,
        Guid? organisationId = null,
        string? assignedOrganisationScopes = null,
        Action<IServiceCollection>? serviceCollection = null)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (serviceCollection != null) builder.ConfigureServices(serviceCollection);

        builder.ConfigureServices(services =>
        {
            services.AddTransient<IPolicyEvaluator>(sp => new AuthorizationPolicyEvaluator(
                ActivatorUtilities.CreateInstance<PolicyEvaluator>(sp), channel, assignedOrganisationScopes));

            if (assignedOrganisationScopes != null && organisationId != null)
            {
                Mock<IOrganisationRepository> mockDatabaseOrgRepo = new();
                mockDatabaseOrgRepo.Setup(r => r.FindOrganisationPerson(organisationId.Value, "urn:fake_user"))
                    .ReturnsAsync(new OrganisationPerson
                    {
                        Person = Mock.Of<Person>(),
                        Organisation = Mock.Of<Organisation>(),
                        Scopes = [assignedOrganisationScopes]
                    });

                services.AddTransient(sc => mockDatabaseOrgRepo.Object);
            }
        });

        return base.CreateHost(builder);
    }
}

public class AuthorizationPolicyEvaluator(PolicyEvaluator innerEvaluator, string? channel, string? assignedOrganisationScopes) : IPolicyEvaluator
{
    const string JwtBearerOrApiKeyScheme = "JwtBearer_Or_ApiKey";

    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var claimsIdentity = new ClaimsIdentity(JwtBearerOrApiKeyScheme);
        if (!string.IsNullOrWhiteSpace(channel)) claimsIdentity.AddClaims([new Claim("channel", channel)]);
        if (assignedOrganisationScopes != null) claimsIdentity.AddClaim(new Claim("sub", "urn:fake_user"));

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties(), JwtBearerOrApiKeyScheme)));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return innerEvaluator.AuthorizeAsync(policy, authenticationResult, context, resource);
    }
}