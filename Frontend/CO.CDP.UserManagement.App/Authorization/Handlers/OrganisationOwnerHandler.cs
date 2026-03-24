using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using ApiClient = CO.CDP.UserManagement.WebApiClient;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.App.Authorization.Requirements;

namespace CO.CDP.UserManagement.App.Authorization.Handlers;

public sealed class OrganisationOwnerHandler : AuthorizationHandler<OrganisationOwnerRequirement>
{
    private readonly ApiClient.UserManagementClient _apiClient;

    public OrganisationOwnerHandler(ApiClient.UserManagementClient apiClient) => _apiClient = apiClient;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganisationOwnerRequirement requirement)
    {
        var sub = context.User?.FindFirst("sub")?.Value;
        var mvcContext = context.Resource as AuthorizationFilterContext;
        var organisationSlug = mvcContext?.RouteData?.Values["organisationSlug"] as string;

        if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(organisationSlug))
        {
            return;
        }

        try
        {
            var org = await _apiClient.BySlugAsync(organisationSlug);
            var user = await _apiClient.UsersGET3Async(org.CdpOrganisationGuid, sub);
            if (user?.OrganisationRole == OrganisationRole.Owner)
            {
                context.Succeed(requirement);
            }
        }
        catch (ApiClient.ApiException)
        {
            // treat not found or other API errors as unauthorized
        }
    }
}
