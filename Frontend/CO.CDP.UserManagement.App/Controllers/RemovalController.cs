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
    public async Task<IActionResult> RemoveUser(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null) return NotFound();

        var validation = await userRemovalService.ValidateRemovalAsync(organisationSlug, cdpPersonId, ct);
        if (!validation.IsValid)
            ModelState.AddModelError(string.Empty, validation.ErrorMessage!);

        return View(viewModel);
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
            return viewModel is null ? NotFound() : View(nameof(RemoveUser), viewModel);
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

        return View(viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}/remove")]
    public async Task<IActionResult> RemoveInvite(string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        return viewModel is null ? NotFound() : View(nameof(RemoveUser), viewModel);
    }

    [HttpPost("invites/{inviteGuid:guid}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveInvite(
        string organisationSlug, Guid inviteGuid, RemoveUserViewModel input, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (viewModel is null) return NotFound();

        if (input.RemoveConfirmed == false)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug });
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(RemoveUser), viewModel);
        }

        var result = await userRemovalService.RemoveInviteAsync(organisationSlug, inviteGuid, ct);

        if (result is InviteRemovalSubmitResult.NotFound) return NotFound();

        await removeInviteStateStore.SetAsync(new RemoveInviteSuccessState
        {
            OrganisationSlug = organisationSlug,
            UserDisplayName = viewModel.UserDisplayName,
            Email = viewModel.Email,
            OrganisationName = viewModel.OrganisationName,
            MemberSince = viewModel.MemberSinceFormatted,
            Role = viewModel.CurrentRole
        });

        return RedirectToAction(nameof(RemoveInviteSuccess), new { organisationSlug, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/remove/success")]
    public async Task<IActionResult> RemoveInviteSuccess(string organisationSlug, Guid inviteGuid)
    {
        var state = await removeInviteStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug });
        }

        await removeInviteStateStore.ClearAsync();

        var viewModel = new RemoveSuccessViewModel
        {
            OrganisationSlug = organisationSlug,
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
        string organisationSlug, Guid cdpPersonId, string clientId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetRemoveApplicationViewModelAsync(
            organisationSlug, cdpPersonId, clientId, ct);
        return viewModel is null ? NotFound() : View(viewModel);
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
            return viewModel is null ? NotFound() : View(viewModel);
        }

        var result = await userRemovalService.RemoveApplicationAsync(organisationSlug, cdpPersonId, clientId, ct);

        if (result is ApplicationRemovalSubmitResult.NotFound) return NotFound();
        return RedirectToAction(nameof(RemoveApplicationSuccess), new { organisationSlug, cdpPersonId, clientId });
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

        return View(viewModel);
    }
}
