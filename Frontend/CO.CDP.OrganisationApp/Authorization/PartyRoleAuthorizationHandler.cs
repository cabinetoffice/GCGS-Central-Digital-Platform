using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class PartyRoleAuthorizationHandler(ISession session, IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PartyRoleAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PartyRoleAuthorizationRequirement requirement)
    {
        UserDetails? userDetails = session.Get<UserDetails>(Session.UserDetailsKey);

        if (userDetails?.PersonId == null)
        {
            context.Fail();
            return;
        }

        try
        {
            using var serviceScope = serviceScopeFactory.CreateScope();
            var userInfoService = serviceScope.ServiceProvider.GetRequiredService<IUserInfoService>();

            var userInfo = await userInfoService.GetUserInfo();
            var organisationId = userInfoService.GetOrganisationId();

            if (organisationId == null)
            {
                context.Fail();
                return;
            }

            var organisation = userInfo.Organisations.FirstOrDefault(o => o.Id == organisationId);
            if (organisation == null)
            {
                context.Fail();
                return;
            }

            if (organisation.Roles.Contains(requirement.RequiredRole))
            {
                context.Succeed(requirement);
                return;
            }
        }
        catch
        {
            context.Fail();
            return;
        }

        context.Fail();
    }
}