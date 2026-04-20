using CO.CDP.UserManagement.App.Application.Removal;
using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class RemovalController(
    IUserRemovalService userRemovalService,
    IUserDetailsQueryService userDetailsQueryService,
    IRemoveInviteStateStore removeInviteStateStore) : UsersBaseController
{
    [HttpGet("user/{cdpPersonId:guid}/remove")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetUserViewModelAsync(id, cdpPersonId, ct);
        if (viewModel is null) return NotFound();

        var validation = await userRemovalService.ValidateRemovalAsync(id, cdpPersonId, ct);
        if (!validation.IsValid)
            ModelState.AddModelError(string.Empty, validation.ErrorMessage!);

        return View(nameof(RemoveUser), viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(
        Guid id, Guid cdpPersonId, RemoveUserViewModel input, CancellationToken ct)
    {
        var result =
            await userRemovalService.ValidateAndRemoveUserAsync(id, cdpPersonId, input.RemoveConfirmed,
                ct);

        if (result is UserRemovalSubmitResult.NotFound) return NotFound();
        if (result is UserRemovalSubmitResult.Cancelled)
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
        if (result is UserRemovalSubmitResult.ValidationError ve)
        {
            ModelState.AddModelError(string.Empty, ve.Message);
            var viewModel = await userRemovalService.GetUserViewModelAsync(id, cdpPersonId, ct);
            return viewModel is null ? NotFound() : View(nameof(RemoveUser), viewModel);
        }

        return RedirectToAction(nameof(RemoveSuccess), new { id, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/remove/success")]
    public async Task<IActionResult> RemoveSuccess(Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetRemoveSuccessViewModelAsync(id, cdpPersonId, ct);
        if (viewModel is null)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
        }

        return View(nameof(RemoveSuccess), viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}/remove")]
    public async Task<IActionResult> RemoveInvite(Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetInviteViewModelAsync(id, inviteGuid, ct);
        return viewModel is null ? NotFound() : View(nameof(RemoveUser), viewModel);
    }

    [HttpPost("invites/{inviteGuid:guid}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveInvite(
        Guid id, Guid inviteGuid, RemoveUserViewModel input, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetInviteViewModelAsync(id, inviteGuid, ct);
        if (viewModel is null) return NotFound();

        if (input.RemoveConfirmed == false)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(RemoveUser), viewModel);
        }

        var result = await userRemovalService.RemoveInviteAsync(id, inviteGuid, ct);

        if (result is InviteRemovalSubmitResult.NotFound) return NotFound();

        await removeInviteStateStore.SetAsync(new RemoveInviteSuccessState
        {
            OrganisationId = id,
            UserDisplayName = viewModel.UserDisplayName,
            Email = viewModel.Email,
            OrganisationName = viewModel.OrganisationName,
            MemberSince = viewModel.MemberSinceFormatted,
            Role = viewModel.CurrentRole
        });

        return RedirectToAction(nameof(RemoveInviteSuccess), new { id, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/remove/success")]
    public async Task<IActionResult> RemoveInviteSuccess(Guid id, Guid inviteGuid)
    {
        var state = await removeInviteStateStore.GetAsync();
        if (state is null || state.OrganisationId != id)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
        }

        await removeInviteStateStore.ClearAsync();

        var viewModel = new RemoveSuccessViewModel
        {
            OrganisationId = id,
            UserDisplayName = state.UserDisplayName,
            Email = state.Email,
            OrganisationName = state.OrganisationName,
            MemberSince = state.MemberSince,
            Role = state.Role,
            CdpPersonId = Guid.Empty
        };

        return View(nameof(RemoveSuccess), viewModel);
    }

    [HttpGet("user/{cdpPersonId:guid}/application/{clientId}/remove")]
    public async Task<IActionResult> RemoveApplication(
        Guid id, Guid cdpPersonId, string clientId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetRemoveApplicationViewModelAsync(
            id, cdpPersonId, clientId, ct);
        return viewModel is null ? NotFound() : View(nameof(RemoveApplication), viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/application/{clientId}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveApplication(
        Guid id, Guid cdpPersonId, string clientId,
        RemoveApplicationViewModel input, CancellationToken ct)
    {
        if (input.RevokeConfirmed == false)
        {
            return RedirectToAction(nameof(UserDetailsController.Details), "UserDetails",
                new { id, cdpPersonId });
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userRemovalService.GetRemoveApplicationViewModelAsync(
                id, cdpPersonId, clientId, ct);
            return viewModel is null ? NotFound() : View(nameof(RemoveApplication), viewModel);
        }

        var result = await userRemovalService.RemoveApplicationAsync(id, cdpPersonId, clientId, ct);

        if (result is ApplicationRemovalSubmitResult.NotFound) return NotFound();
        return RedirectToAction(nameof(RemoveApplicationSuccess), new { id, cdpPersonId, clientId });
    }

    [HttpGet("user/{cdpPersonId:guid}/application/{clientId}/remove/success")]
    public async Task<IActionResult> RemoveApplicationSuccess(
        Guid id, Guid cdpPersonId, string clientId, CancellationToken ct)
    {
        var viewModel = await userDetailsQueryService.GetRemoveApplicationSuccessViewModelAsync(
            id, cdpPersonId, clientId, ct);

        if (viewModel is null)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
        }

        return View(nameof(RemoveApplicationSuccess), viewModel);
    }
}
