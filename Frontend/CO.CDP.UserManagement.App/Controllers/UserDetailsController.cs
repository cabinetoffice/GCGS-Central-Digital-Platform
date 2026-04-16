using CO.CDP.UserManagement.App.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class UserDetailsController(
    IUserDetailsQueryService userDetailsQueryService,
    IInviteDetailsQueryService inviteDetailsQueryService) : UsersBaseController
{
    [HttpGet("user/{cdpPersonId:guid}")]
    public async Task<IActionResult> Details(Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userDetailsQueryService.GetViewModelAsync(id, cdpPersonId, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}")]
    public async Task<IActionResult> InviteDetails(Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await inviteDetailsQueryService.GetViewModelAsync(id, inviteGuid, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }
}
