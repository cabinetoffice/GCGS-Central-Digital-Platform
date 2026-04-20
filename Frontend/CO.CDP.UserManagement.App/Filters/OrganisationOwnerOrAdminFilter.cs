using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CO.CDP.UserManagement.App.Filters;

public class OrganisationOwnerOrAdminFilter(
    ICurrentUserService currentUserService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.RouteData.Values.TryGetValue("id", out var routeValue) ||
            !TryGetOrganisationId(routeValue, out var organisationId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var role = currentUserService.GetOrganisationRole(organisationId);
        if (role is not (OrganisationRole.Owner or OrganisationRole.Admin))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }

    private static bool TryGetOrganisationId(object? routeValue, out Guid organisationId)
    {
        switch (routeValue)
        {
            case Guid id:
                organisationId = id;
                return true;
            case string value when Guid.TryParse(value, out var parsedId):
                organisationId = parsedId;
                return true;
            default:
                organisationId = Guid.Empty;
                return false;
        }
    }
}
