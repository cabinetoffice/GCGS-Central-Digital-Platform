using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class OrganizationScopeHandler : AuthorizationHandler<OrganizationScopeRequirement>
{
    private ITenantClient _tenantClient;
    private ISession _session;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrganizationScopeHandler(ITenantClient tenantClient, ISession session, IHttpContextAccessor httpContextAccessor)
    {
        _tenantClient = tenantClient;
        _session = session;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganizationScopeRequirement requirement)
    {
        Models.UserDetails? userDetails = _session.Get<Models.UserDetails>(Session.UserDetailsKey);

        if (userDetails != null && userDetails.PersonId != null)
        {
            try
            {
                Guid? organisationId = GetOrganisationId();

                if (organisationId != null)
                {
                    UserOrganisation? personOrganisation = await GetPersonOrganisation((Guid)organisationId);

                    if (personOrganisation != null && personOrganisation.Scopes.Contains(requirement.Scope))
                    {
                        context.Succeed(requirement);
                        return;
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
        // Role checks may be made multiple times when building a page
        // Therefore we cache the person's organisation details for the duration of the http request
        var cacheKey = "CO.CDP.OrganisationApp.Authorization.OrganizationScopeHandler.GetPersonOrganisation";

        if (_httpContextAccessor?.HttpContext?.Items != null && _httpContextAccessor.HttpContext.Items[cacheKey] is UserOrganisation cachedData)
        {
            return cachedData;
        }

        var result = await _tenantClient.LookupTenantAsync();

        var personOrganisation = result.Tenants
            .SelectMany(tenant => tenant.Organisations)
            .FirstOrDefault(org => org.Id == organisationId);

        if(_httpContextAccessor?.HttpContext?.Items != null)
        {
            _httpContextAccessor.HttpContext.Items[cacheKey] = personOrganisation;
        }

        return personOrganisation;
    }

    private Guid? GetOrganisationId()
    {
        if (_httpContextAccessor?.HttpContext?.Request?.Path.Value == null)
        {
            return null;
        }

        var path = _httpContextAccessor.HttpContext.Request.Path.Value;

        if (path != null)
        {
            var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length >= 2 && pathSegments[0] == "organisation" && Guid.TryParse(pathSegments[1], out Guid organisationId))
            {
                return organisationId;
            }
        }

        return null;
    }
}