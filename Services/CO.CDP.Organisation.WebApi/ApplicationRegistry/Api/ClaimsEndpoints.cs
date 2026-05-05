using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class ClaimsEndpoints
{
    public static void UseClaimsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/claims/{userPrincipalId}", async (
            string userPrincipalId,
            IUseCase<string, ClaimsTree> useCase) =>
        {
            var result = await useCase.Execute(userPrincipalId);
            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Claims");

        app.MapGet("/api/claims/{userPrincipalId}/organisations/{orgId:guid}", async (
            string userPrincipalId,
            Guid orgId,
            IUseCase<string, ClaimsTree> useCase) =>
        {
            var tree = await useCase.Execute(userPrincipalId);
            var orgClaims = tree.Organisations.FirstOrDefault(o => o.OrganisationId == orgId);
            return orgClaims != null ? Results.Ok(orgClaims) : Results.NotFound();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgMember)
        .WithTags("Claims");
    }
}
