using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp;

public class UserInfoService(IHttpContextAccessor httpContextAccessor, ITenantClient tenantClient) : IUserInfoService
{
    public async Task<ICollection<string>> GetOrganisationUserScopes()
    {
        Guid? organisationId = GetOrganisationId();
        if (organisationId != null)
        {
            var organisation = await GetPersonOrganisation((Guid)organisationId);

            if(organisation != null)
            {
                return organisation.Scopes;
            }            
        }

        return [];
    }

    public async Task<bool> UserHasScope(string scope)
    {
        var scopes = await GetOrganisationUserScopes();
        return scopes.Contains(scope);
    }

    private async Task<UserOrganisation?> GetPersonOrganisation(Guid organisationId)
    {
        // Role checks may be made multiple times when building a page
        // Therefore we cache the person's organisation details for the duration of the http request
        var cacheKey = "CO.CDP.OrganisationApp.UserInfoService.GetPersonOrganisation";

        if (httpContextAccessor?.HttpContext?.Items != null && httpContextAccessor.HttpContext.Items[cacheKey] is UserOrganisation cachedData)
        {
            return cachedData;
        }

        var result = await tenantClient.LookupTenantAsync();

        var personOrganisation = result.Tenants
            .SelectMany(tenant => tenant.Organisations)
            .FirstOrDefault(org => org.Id == organisationId);

        if (httpContextAccessor?.HttpContext?.Items != null)
        {
            httpContextAccessor.HttpContext.Items[cacheKey] = personOrganisation;
        }

        return personOrganisation;
    }

    public Guid? GetOrganisationId()
    {
        if (httpContextAccessor?.HttpContext?.Request?.Path.Value == null)
        {
            return null;
        }

        var path = httpContextAccessor.HttpContext.Request.Path.Value;

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
