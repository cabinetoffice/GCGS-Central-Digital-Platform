using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.UserManagement.App.Services;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize]
[Route("organisation/{organisationSlug}/users")]
public class UsersController(IUserService userService) : Controller
{
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Index(
        string? organisationSlug,
        string? role,
        string? application,
        string? search,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrEmpty(organisationSlug))
        {
            return NotFound();
        }

        var viewModel = await userService.GetUsersViewModelAsync(organisationSlug, role, application, search, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
