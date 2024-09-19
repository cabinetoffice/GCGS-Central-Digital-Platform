using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.OrganisationApp.Authorization;

public class OrganizationScopeHandler : AuthorizationHandler<OrganizationScopeRequirement>
{
    private ITenantClient _tenantClient;
    private ISession _session;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public OrganizationScopeHandler(ITenantClient tenantClient, ISession session, IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory)
    {
        _tenantClient = tenantClient;
        _session = session;
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationScopeRequirement requirement)
    {
        Models.UserDetails? userDetails = _session.Get<Models.UserDetails>(Session.UserDetailsKey);

        if (userDetails != null && userDetails.PersonId != null)
        {
            try
            {
                // The UserInfoService is scoped, but authorization is a singleton, so we need to work around that with a ServiceScopeFactory
                using (var serviceScope = _serviceScopeFactory.CreateScope())
                {
                    IUserInfoService _userInfo = serviceScope.ServiceProvider.GetRequiredService<IUserInfoService>();

                    var scopes = await _userInfo.GetOrganisationUserScopes();

                    if (scopes.Contains(requirement.Scope))
                    {
                        context.Succeed(requirement);
                        return;
                    }

                    // Editor role implies viewer permissions also
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