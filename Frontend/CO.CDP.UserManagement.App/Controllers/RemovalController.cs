using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Application.Removal;
using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class RemovalController(
    IUserRemovalService userRemovalService,
    IUserDetailsQueryService userDetailsQueryService) : UsersBaseController
{
    [HttpGet("user/{cdpPersonId:guid}/remove")]
    public async Task<IActionResult> RemoveUser(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null) return NotFound();

        if (await userRemovalService.IsLastOwnerAsync(organisationSlug, cdpPersonId, ct))
            ModelState.AddModelError(string.Empty, "You cannot remove the last owner of the organisation.");

        return View("~/Views/Users/Remove.cshtml", viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(
        string organisationSlug, Guid cdpPersonId, RemoveUserViewModel input, CancellationToken ct)
    {
        var result = await userRemovalService.ValidateAndRemoveUserAsync(organisationSlug, cdpPersonId, input.RemoveConfirmed, ct);

        if (result is UserRemovalSubmitResult.NotFound) return NotFound();
        if (result is UserRemovalSubmitResult.Cancelled)
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug });
        if (result is UserRemovalSubmitResult.ValidationError ve)
        {
            ModelState.AddModelError(string.Empty, ve.Message);
            var viewModel = await userRemovalService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
            return viewModel is null ? NotFound() : View("~/Views/Users/Remove.cshtml", viewModel);
        }

        return RedirectToAction(nameof(RemoveSuccess), new { organisationSlug, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/remove/success")]
    public async Task<IActionResult> RemoveSuccess(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetRemoveSuccessViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug });
        }

        return View("~/Views/Users/RemoveSuccess.cshtml", viewModel);
    }

    [HttpGet("invites/{pendingInviteId:int}/remove")]
    public async Task<IActionResult> RemoveInvite(string organisationSlug, int pendingInviteId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetInviteViewModelAsync(organisationSlug, pendingInviteId, ct);
        return viewModel is null ? NotFound() : View("~/Views/Users/Remove.cshtml", viewModel);
    }

    [HttpPost("invites/{pendingInviteId:int}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveInvite(
        string organisationSlug, int pendingInviteId, RemoveUserViewModel input, CancellationToken ct)
    {
        if (input.RemoveConfirmed == false)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug });
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userRemovalService.GetInviteViewModelAsync(organisationSlug, pendingInviteId, ct);
            return viewModel is null ? NotFound() : View("~/Views/Users/Remove.cshtml", viewModel);
        }

        var result = await userRemovalService.RemoveInviteAsync(organisationSlug, pendingInviteId, ct);
        var success = result.Match(_ => false, outcome => outcome == ServiceOutcome.Success);
        return success
            ? RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug })
            : NotFound();
    }

    [HttpGet("user/{cdpPersonId:guid}/application/{clientId}/remove")]
    public async Task<IActionResult> RemoveApplication(
        string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetRemoveApplicationViewModelAsync(
            organisationSlug, cdpPersonId, clientId, ct);
        return viewModel is null ? NotFound() : View("~/Views/Users/RemoveApplication.cshtml", viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/application/{clientId}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveApplication(
        string organisationSlug, Guid cdpPersonId, string clientId,
        RemoveApplicationViewModel input, CancellationToken ct)
    {
        if (input.RevokeConfirmed == false)
        {
            return RedirectToAction(nameof(UserDetailsController.Details), "UserDetails",
                new { organisationSlug, cdpPersonId });
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userRemovalService.GetRemoveApplicationViewModelAsync(
                organisationSlug, cdpPersonId, clientId, ct);
            return viewModel is null ? NotFound() : View("~/Views/Users/RemoveApplication.cshtml", viewModel);
        }

        var result = await userRemovalService.RemoveApplicationAsync(organisationSlug, cdpPersonId, clientId, ct);
        return result.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(RemoveApplicationSuccess), new
                {
                    organisationSlug,
                    cdpPersonId,
                    clientId
                }));
    }

    [HttpGet("user/{cdpPersonId:guid}/application/{clientId}/remove/success")]
    public async Task<IActionResult> RemoveApplicationSuccess(
        string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct)
    {
        var viewModel = await userDetailsQueryService.GetRemoveApplicationSuccessViewModelAsync(
            organisationSlug, cdpPersonId, clientId, ct);

        if (viewModel is null)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug });
        }

        return View("~/Views/Users/RemoveApplicationSuccess.cshtml", viewModel);
    }
}
