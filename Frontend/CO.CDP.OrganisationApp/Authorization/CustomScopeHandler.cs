using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
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
                    var tenantClient = serviceScope.ServiceProvider.GetRequiredService<ITenantClient>();

                    var tenantLookup = await tenantClient.LookupTenantAsync();
                    var userScopes = tenantLookup.User.Scopes;
                    var organisationUserScopes = tenantLookup.Tenants.SelectMany(x => x.Organisations.SelectMany(y => y.Scopes)).ToList();

                    // Support admin implies viewer permissions
                    if (requirement.Scope == OrganisationPersonScopes.Viewer && userScopes.Contains(PersonScopes.SupportAdmin))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    if (requirement.Scope == PersonScopes.SupportAdmin && userScopes.Contains(PersonScopes.SupportAdmin))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    // Admin role can do anything within this organisation
                    if (organisationUserScopes.Contains(OrganisationPersonScopes.Admin) && requirement.Scope != PersonScopes.SupportAdmin)
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
            catch
            {
                context.Fail();
            }
        }

        context.Fail();
    }
}