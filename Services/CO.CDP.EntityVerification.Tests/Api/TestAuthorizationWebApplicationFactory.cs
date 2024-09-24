using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace CO.CDP.EntityVerification.Tests.Api;

public class TestAuthorizationWebApplicationFactory<TProgram>(
        string channel,
        Action<IServiceCollection>? serviceCollection = null)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (serviceCollection != null) builder.ConfigureServices(serviceCollection);

        builder.ConfigureServices(services =>
        {
            services.AddTransient<IPolicyEvaluator>(sp => new AuthorizationPolicyEvaluator(
                ActivatorUtilities.CreateInstance<PolicyEvaluator>(sp), channel));
        });

        return base.CreateHost(builder);
    }
}

public class AuthorizationPolicyEvaluator(PolicyEvaluator innerEvaluator, string? channel) : IPolicyEvaluator
{
    const string JwtBearerScheme = "Bearer";

    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var claimsIdentity = new ClaimsIdentity(JwtBearerScheme);
        if (!string.IsNullOrWhiteSpace(channel)) claimsIdentity.AddClaims([new Claim("channel", channel)]);

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties(), JwtBearerScheme)));
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return innerEvaluator.AuthorizeAsync(policy, authenticationResult, context, resource);
    }
}