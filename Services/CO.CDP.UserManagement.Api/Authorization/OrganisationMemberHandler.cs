using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.UserManagement.Api.Authorization;

/// <summary>
/// Authorization handler for verifying organisation membership.
/// </summary>
public class OrganisationMemberHandler : AuthorizationHandler<OrganisationMemberRequirement>
{
    private readonly ILogger<OrganisationMemberHandler> _logger;
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;

    public OrganisationMemberHandler(
        ILogger<OrganisationMemberHandler> logger,
        IOrganisationRepository organisationRepository,
        IUserOrganisationMembershipRepository membershipRepository)
    {
        _logger = logger;
        _organisationRepository = organisationRepository;
        _membershipRepository = membershipRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganisationMemberRequirement requirement)
    {
        var membershipContextResult = await OrganisationAuthorizationContextHelper.GetMembershipContextAsync(
            context,
            _organisationRepository,
            _membershipRepository);

        membershipContextResult.Match(
            reason => _logger.LogInformation(
                "Denied organisation member authorisation: {Reason}",
                reason.Code),
            state =>
            {
                if (state.Membership.IsActive)
                {
                    _logger.LogInformation(
                        "Authorised organisation member {UserPrincipalId} for organisation {CdpOrganisationId}",
                        state.UserPrincipalId.Value,
                        state.CdpOrganisationId.Value);
                    context.Succeed(requirement);
                }
            });
    }
}

/// <summary>
/// Requirement for organisation membership authorization.
/// </summary>
public class OrganisationMemberRequirement : IAuthorizationRequirement
{
}
