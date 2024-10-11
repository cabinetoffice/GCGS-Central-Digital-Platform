using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp;

public class UserInfoService(IHttpContextAccessor httpContextAccessor, ITenantClient tenantClient, IPersonClient personClient) : IUserInfoService
{
    public async Task<ICollection<string>> GetUserScopes()
    {
        var userUrn = GetUserUrn();
        if (userUrn == null)
        {
            return new List<string>(); // Return an empty list if no userUrn is found
        }

        var person = await personClient.LookupPersonAsync(userUrn);

        if (person == null || person.Scopes == null)
        {
            return new List<string>(); // Return an empty list if no person or roles are found
        }

        return person.Scopes;
    }

    private string? GetUserUrn()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

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

    public async Task<bool> IsViewer()
    {
        var scopes = await GetOrganisationUserScopes();
        var userScopes = await GetUserScopes();
        return scopes.Contains(OrganisationPersonScopes.Viewer) || userScopes.Contains(PersonScopes.SupportAdmin);
    }

    public async Task<bool> HasTenant()
    {
        try
        {
            var usersTenant = await tenantClient.LookupTenantAsync();
            return usersTenant.Tenants.Count > 0;
        }
        catch (CO.CDP.Tenant.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }    
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
