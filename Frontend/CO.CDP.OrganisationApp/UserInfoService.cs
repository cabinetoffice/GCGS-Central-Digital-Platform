using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp;

public class UserInfoService(IHttpContextAccessor httpContextAccessor, ITenantClient tenantClient) : IUserInfoService
{
    public async Task<bool> IsViewer()
    {
        var tenantLookup = await tenantClient.LookupTenantAsync();
        var userScopes = tenantLookup.User.Scopes;
        var organisationId = GetOrganisationId();
        var organisationUserScopes = tenantLookup.Tenants.SelectMany(x => x.Organisations.Where(y => y.Id == organisationId).SelectMany(y => y.Scopes)).ToList();

        return organisationUserScopes.Contains(OrganisationPersonScopes.Viewer) || (organisationUserScopes.Count == 0 && userScopes.Contains(PersonScopes.SupportAdmin));
    }

    public async Task<bool> HasTenant()
    {
        try
        {
            var usersTenant = await tenantClient.LookupTenantAsync();
            return usersTenant.Tenants.Count > 0;
        }
        catch
        {
            return false;
        }
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
