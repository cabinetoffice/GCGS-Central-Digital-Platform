//using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class OrganizationRoleHandler : AuthorizationHandler<OrganizationRoleRequirement>
{
    private ITenantClient _tenantClient;
    private ISession _session;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationRoleHandler(ITenantClient tenantClient, ISession session, IHttpContextAccessor httpContextAccessor)
    {
        _tenantClient = tenantClient;
        _session = session;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationRoleRequirement requirement)
    {
        Models.UserDetails? userDetails = _session.Get<Models.UserDetails>(Session.UserDetailsKey);

        if (userDetails != null && userDetails.PersonId != null)
        {
            try
            {

                var path = _httpContextAccessor.HttpContext.Request.Path.Value;

                if (path != null)
                {
                    var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (pathSegments.Length >= 2 && pathSegments[0] == "organisation" && Guid.TryParse(pathSegments[1], out Guid organisationId))
                    {
                        UserOrganisation? personOrganisation = await GetPersonOrganisation(organisationId);

                        if (personOrganisation != null && personOrganisation.Scopes.Contains(requirement.Role))
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Fail();
            }
        }

        context.Fail();

        // TODO: Stop the app throwing 404 page-not-found when auth fails - need to throw a 403?
    }

    private async Task<UserOrganisation?> GetPersonOrganisation(Guid organisationId)
    {
        var cacheKey = "CO.CDP.OrganisationApp.Authorization.OrganizationRoleHandler.GetPersonOrganisation";

        if (_httpContextAccessor.HttpContext.Items[cacheKey] is UserOrganisation cachedData)
        {
            return cachedData;
        }

        var result = await _tenantClient.LookupTenantAsync();

        var personOrganisation = result.Tenants
            .SelectMany(tenant => tenant.Organisations)
            .FirstOrDefault(org => org.Id == organisationId);

        _httpContextAccessor.HttpContext.Items[cacheKey] = personOrganisation;

        return personOrganisation;
    }
}