using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class OrganizationScopeHandler(
    ISession session,
    IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<OrganizationScopeRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationScopeRequirement requirement)
    {
        Models.UserDetails? userDetails = session.Get<Models.UserDetails>(Session.UserDetailsKey);

        if (userDetails != null && userDetails.PersonId != null)
        {
            try
            {
                // The UserInfoService is scoped, but authorization is a singleton, so we need to work around that with a ServiceScopeFactory
                using (var serviceScope = serviceScopeFactory.CreateScope())
                {
                    IUserInfoService _userInfo = serviceScope.ServiceProvider.GetRequiredService<IUserInfoService>();

                    var scopes = await _userInfo.GetOrganisationUserScopes();

                    // Admin role can do anything within this organisation
                    if (scopes.Contains(OrganisationPersonScopes.Admin))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    if (scopes.Contains(requirement.Scope))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    // Editor role implies viewer permissions
                    if (requirement.Scope == OrganisationPersonScopes.Viewer && scopes.Contains(OrganisationPersonScopes.Editor))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
            catch
            {
                context.Fail();
            }
        }

        context.Fail();
    }
}