using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.OrganisationApp.Tests;
public class FakeAuthenticationPolicyEvaluator : IPolicyEvaluator
{

    // This fake policy evaluator allows us to bypass the OneLogin authentication
    // However the AuthorizeAsync method is designed to still call the original authorization service
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
        var policyEvaluator = context.RequestServices.GetRequiredService<IAuthorizationService>();
        var result = await policyEvaluator.AuthorizeAsync(context.User, resource, policy);

        return result.Succeeded
            ? PolicyAuthorizationResult.Success()
            : PolicyAuthorizationResult.Forbid();
    }
}