using CO.CDP.UserManagement.App.Application.OrganisationRoles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CO.CDP.UserManagement.App.Filters;

public class OrganisationOwnerOrAdminFilter(IOrganisationRoleFlowService organisationRoleFlowService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sub = (context.Controller as ControllerBase)?.User.FindFirst("sub")?.Value;
        var organisationSlug = context.RouteData.Values["organisationSlug"] as string;

        if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(organisationSlug))
        {
            context.Result = new ForbidResult();
            return;
        }

        var cancellationToken = context.HttpContext.RequestAborted;
        var isAuthorised = await organisationRoleFlowService.IsOwnerOrAdminAsync(organisationSlug, sub, cancellationToken);
        if (!isAuthorised)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
