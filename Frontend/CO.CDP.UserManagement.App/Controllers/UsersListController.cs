using CO.CDP.UserManagement.App.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class UsersListController(IUsersQueryService usersQueryService) : UsersBaseController
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        Guid id,
        string? role,
        string? application,
        string? search,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var viewModel = await usersQueryService.GetViewModelAsync(id, role, application, search, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
