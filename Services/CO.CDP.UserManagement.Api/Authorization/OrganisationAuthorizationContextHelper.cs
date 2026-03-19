using CO.CDP.Functional;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CO.CDP.UserManagement.Api.Authorization;

internal readonly record struct OrganisationAuthorizationFailure(string Code)
{
    public static readonly OrganisationAuthorizationFailure InvalidCdpOrganisationId = new("invalid-cdp-organisation-id");
    public static readonly OrganisationAuthorizationFailure MissingUserPrincipalId = new("missing-user-principal-id");
    public static readonly OrganisationAuthorizationFailure UnauthenticatedUser = new("unauthenticated-user");
    public static readonly OrganisationAuthorizationFailure OrganisationNotFound = new("organisation-not-found");
    public static readonly OrganisationAuthorizationFailure MembershipNotFound = new("membership-not-found");

    public override string ToString() => Code;
}

internal readonly record struct CdpOrganisationId(Guid Value)
{
    public override string ToString() => Value.ToString();
}

internal readonly record struct UserPrincipalId(string Value)
{
    public override string ToString() => Value;
}

internal readonly record struct ResolvedOrganisationMembershipContext(
    CdpOrganisationId CdpOrganisationId,
    UserPrincipalId UserPrincipalId,
    UserOrganisationMembership Membership);

internal static class OrganisationAuthorizationContextHelper
{
    public static Result<OrganisationAuthorizationFailure, CdpOrganisationId> GetCdpOrganisationId(AuthorizationHandlerContext context) =>
        context.Resource is HttpContext httpContext
        && Guid.TryParse(httpContext.Request.RouteValues["cdpOrganisationId"]?.ToString(), out var cdpOrganisationId)
            ? Result<OrganisationAuthorizationFailure, CdpOrganisationId>.Success(new CdpOrganisationId(cdpOrganisationId))
            : Result<OrganisationAuthorizationFailure, CdpOrganisationId>.Failure(OrganisationAuthorizationFailure.InvalidCdpOrganisationId);

    public static Result<OrganisationAuthorizationFailure, UserPrincipalId> GetUserPrincipalId(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true
            ? (user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("user_id")?.Value)
                is { Length: > 0 } userPrincipalId
                ? Result<OrganisationAuthorizationFailure, UserPrincipalId>.Success(new UserPrincipalId(userPrincipalId))
                : Result<OrganisationAuthorizationFailure, UserPrincipalId>.Failure(OrganisationAuthorizationFailure.MissingUserPrincipalId)
            : Result<OrganisationAuthorizationFailure, UserPrincipalId>.Failure(OrganisationAuthorizationFailure.UnauthenticatedUser);

    public static async Task<Result<OrganisationAuthorizationFailure, ResolvedOrganisationMembershipContext>> GetMembershipContextAsync(
        AuthorizationHandlerContext context,
        IOrganisationRepository organisationRepository,
        IUserOrganisationMembershipRepository membershipRepository)
    {
        var cdpOrganisationIdResult = GetCdpOrganisationId(context);
        var userPrincipalIdResult = GetUserPrincipalId(context.User);

        return await cdpOrganisationIdResult.MatchAsync(
            failure => Task.FromResult(Result<OrganisationAuthorizationFailure, ResolvedOrganisationMembershipContext>.Failure(failure)),
            async cdpOrganisationId => await userPrincipalIdResult.MatchAsync(
                failure => Task.FromResult(Result<OrganisationAuthorizationFailure, ResolvedOrganisationMembershipContext>.Failure(failure)),
                async userPrincipalId =>
                {
                    var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId.Value);

                    if (organisation == null)
                    {
                        return Result<OrganisationAuthorizationFailure, ResolvedOrganisationMembershipContext>.Failure(OrganisationAuthorizationFailure.OrganisationNotFound);
                    }

                    var membership = await membershipRepository.GetByUserAndOrganisationAsync(userPrincipalId.Value, organisation.Id);

                    return membership != null
                        ? Result<OrganisationAuthorizationFailure, ResolvedOrganisationMembershipContext>.Success(
                            new ResolvedOrganisationMembershipContext(cdpOrganisationId, userPrincipalId, membership))
                        : Result<OrganisationAuthorizationFailure, ResolvedOrganisationMembershipContext>.Failure(OrganisationAuthorizationFailure.MembershipNotFound);
                }));
    }
}
