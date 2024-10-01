using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class CustomScopeHandler(
    ISession session,
    IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<ScopeRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
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

                    var userScopes = await _userInfo.GetUserScopes();

                    // SupportAdmin role can do anything within any organisation
                    if (userScopes.Contains(PersonScopes.SupportAdmin))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    var organisationUserScopes = await _userInfo.GetOrganisationUserScopes();

                    // Admin role can do anything within this organisation
                    if (organisationUserScopes.Contains(OrganisationPersonScopes.Admin))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    if (organisationUserScopes.Contains(requirement.Scope))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    // Editor role implies viewer permissions
                    if (requirement.Scope == OrganisationPersonScopes.Viewer && organisationUserScopes.Contains(OrganisationPersonScopes.Editor))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                context.Fail();
            }
        }

        context.Fail();
    }
}