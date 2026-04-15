using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CO.CDP.UserManagement.App.Filters;

public class OrganisationOwnerOrAdminFilter(
    IUserManagementApiAdapter apiAdapter,
    ICurrentUserService currentUserService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var organisationSlug = context.RouteData.Values["organisationSlug"] as string;

        if (string.IsNullOrEmpty(organisationSlug))
        {
            context.Result = new ForbidResult();
            return;
        }

        var org = await apiAdapter.GetOrganisationBySlugAsync(
            organisationSlug, context.HttpContext.RequestAborted);

        if (org is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var role = currentUserService.GetOrganisationRole(org.CdpOrganisationGuid);
        if (role is not (OrganisationRole.Owner or OrganisationRole.Admin))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
