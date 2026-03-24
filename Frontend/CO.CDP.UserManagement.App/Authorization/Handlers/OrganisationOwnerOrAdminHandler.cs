using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Authorization.Requirements;

namespace CO.CDP.UserManagement.App.Authorization.Handlers;

public sealed class OrganisationOwnerOrAdminHandler : AuthorizationHandler<OrganisationOwnerOrAdminRequirement>
{
    private readonly IUserService _userService;

    public OrganisationOwnerOrAdminHandler(IUserService userService) => _userService = userService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrganisationOwnerOrAdminRequirement requirement)
    {
        var sub = context.User?.FindFirst("sub")?.Value;
        var mvcContext = context.Resource as AuthorizationFilterContext;
        var organisationSlug = mvcContext?.RouteData?.Values["organisationSlug"] as string;

        if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(organisationSlug))
        {
            return;
        }

        var isAuthorised = await _userService.IsOwnerOrAdminAsync(organisationSlug, sub);
        if (isAuthorised)
        {
            context.Succeed(requirement);
        }
    }
}
