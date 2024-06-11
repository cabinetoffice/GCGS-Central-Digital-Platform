using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace CO.CDP.TestKit.Mvc;

public class TestWebApplicationFactory<TProgram>(Action<IHostBuilder> configurator)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        configurator(builder);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
        });

        return base.CreateHost(builder);
    }
}

public class FakePolicyEvaluator : IPolicyEvaluator
{
    public async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        var principal = new ClaimsPrincipal();

        principal.AddIdentity(new ClaimsIdentity([new Claim("sub", "fake_sub")], "FakeScheme"));

        return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,
         new AuthenticationProperties(), "FakeScheme")));
    }

    public async Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        return await Task.FromResult(PolicyAuthorizationResult.Success());
    }
}