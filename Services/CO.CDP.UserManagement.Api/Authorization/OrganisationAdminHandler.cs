using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.UserManagement.Api.Authorization;

/// <summary>
/// Authorization handler for verifying organisation admin/owner role.
/// </summary>
public class OrganisationAdminHandler : AuthorizationHandler<OrganisationAdminRequirement>
{
    private readonly ILogger<OrganisationAdminHandler> _logger;
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;

    public OrganisationAdminHandler(
        ILogger<OrganisationAdminHandler> logger,
        IOrganisationRepository organisationRepository,
        IUserOrganisationMembershipRepository membershipRepository)
    {
        _logger = logger;
        _organisationRepository = organisationRepository;
        _membershipRepository = membershipRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationAdminRequirement requirement)
    {
        var membershipContextResult = await OrganisationAuthorizationContextHelper.GetMembershipContextAsync(
            context,
            _organisationRepository,
            _membershipRepository);

        membershipContextResult.Match(
            reason => _logger.LogInformation(
                "Denied organisation admin authorisation: {Reason}",
                reason.Code),
            state =>
            {
                if (state.Membership.IsActive
                    && state.Membership.OrganisationRole is OrganisationRole.Admin or OrganisationRole.Owner)
                {
                    _logger.LogInformation(
                        "Authorised organisation admin {UserPrincipalId} for organisation {CdpOrganisationId} with role {OrganisationRole}",
                        state.UserPrincipalId.Value,
                        state.CdpOrganisationId.Value,
                        state.Membership.OrganisationRole);
                    context.Succeed(requirement);
                }
            });
    }
}

/// <summary>
/// Requirement for organisation admin authorization.
/// </summary>
public class OrganisationAdminRequirement : IAuthorizationRequirement
{
}
