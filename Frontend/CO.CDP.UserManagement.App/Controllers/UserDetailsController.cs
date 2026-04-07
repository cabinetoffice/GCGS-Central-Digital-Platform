using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Application.InviteUsers;
using CO.CDP.UserManagement.App.Application.Users.Implementations;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class UserDetailsController(
    IUserDetailsQueryService userDetailsQueryService,
    IInviteDetailsQueryService inviteDetailsQueryService) : UsersBaseController
{
    [HttpGet("user/{cdpPersonId:guid}")]
    public async Task<IActionResult> Details(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userDetailsQueryService.GetViewModelAsync(organisationSlug, cdpPersonId, ct);
        return viewModel is null ? NotFound() : View("~/Views/Users/Details.cshtml", viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}")]
    public async Task<IActionResult> InviteDetails(string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await inviteDetailsQueryService.GetViewModelAsync(organisationSlug, inviteGuid, ct);
        return viewModel is null ? NotFound() : View("~/Views/Users/InviteDetails.cshtml", viewModel);
    }
}
