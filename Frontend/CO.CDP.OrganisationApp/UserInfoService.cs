using CO.CDP.Functional;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;

namespace CO.CDP.OrganisationApp;

public class UserInfoService(IHttpContextAccessor httpContextAccessor, ITenantClient tenantClient) : IUserInfoService
{
    public async Task<UserInfo> GetUserInfo()
    {
        // Role checks and therefore tenant lookup calls may be made multiple times when building a page.
        // Therefore, we cache the user info for the duration of the http request.
        return await Cached(nameof(UserInfo),
            async () => await tenantClient.LookupTenantAsync().AndThen(MapToUserInfo));
    }

    public async Task<bool> IsViewer()
    {
        var userInfo = await GetUserInfo();
        var userScopes = userInfo.Scopes;
        var organisationId = GetOrganisationId();
        var organisationUserScopes =
            userInfo.Organisations.Where(o => o.Id == organisationId).SelectMany(o => o.Scopes).ToList();

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

    private async Task<T> Cached<T>(string cacheKey, Func<Task<T>> call)
    {
        if (httpContextAccessor.HttpContext?.Items[cacheKey] is T cached)
        {
            return cached;
        }

        var result = await call();
        if (httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Items[cacheKey] = result;
        }
        return result;
    }

    private static UserInfo MapToUserInfo(TenantLookup lookup) => new()
    {
        Name = lookup.User.Name,
        Email = lookup.User.Email,
        Scopes = lookup.User.Scopes,
        Organisations = lookup.Tenants.SelectMany(t =>
            t.Organisations.Select(o => new UserOrganisationInfo
            {
                Id = o.Id,
                Name = o.Name,
                Scopes = o.Scopes,
                Roles = o.Roles,
                PendingRoles = o.PendingRoles
            })).ToList()
    };
}
