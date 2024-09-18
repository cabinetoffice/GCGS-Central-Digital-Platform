using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace CO.CDP.TestKit.Mvc;

public class TestAuthorizationWebApplicationFactory<TProgram>(Claim[] claimFeed)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddTransient<IPolicyEvaluator>(sp => new AuthorizationPolicyEvaluator(
                ActivatorUtilities.CreateInstance<PolicyEvaluator>(sp), claimFeed));
        });

        return base.CreateHost(builder);
    }
}

public class AuthorizationPolicyEvaluator(PolicyEvaluator innerEvaluator, Claim[] claimFeed) : IPolicyEvaluator
{
    const string JwtBearerOrApiKeyScheme = "JwtBearer_Or_ApiKey";

    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var principal = new ClaimsPrincipal();
        if (claimFeed.Length > 0) principal.AddIdentity(new ClaimsIdentity(claimFeed, JwtBearerOrApiKeyScheme));

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,
                    new AuthenticationProperties(), JwtBearerOrApiKeyScheme)));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return innerEvaluator.AuthorizeAsync(policy, authenticationResult, context, resource);
    }
}