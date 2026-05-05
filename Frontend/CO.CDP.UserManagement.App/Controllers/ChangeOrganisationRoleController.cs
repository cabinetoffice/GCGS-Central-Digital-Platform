using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Application.OrganisationRoles;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class ChangeOrganisationRoleController(
    IOrganisationRoleFlowService organisationRoleFlowService) : UsersBaseController
{
    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change")]
    public async Task<IActionResult> ChangeRole(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetUserViewModelAsync(id, cdpPersonId, ct);
        if (viewModel is null) return NotFound();

        var state = await organisationRoleFlowService.GetOrCreateStateAsync(id, cdpPersonId, null, viewModel, ct);
        return View(nameof(ChangeRole), await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, state.SelectedRole, ct));
    }

    [HttpPost("user/{cdpPersonId:guid}/organisation-role/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRoleSubmit(
        Guid id, Guid cdpPersonId, OrganisationRole? organisationRole, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetUserViewModelAsync(id, cdpPersonId, ct);
        if (viewModel is null) return NotFound();

        var result = await organisationRoleFlowService.ValidateAndSaveRoleChangeAsync(
            id, cdpPersonId, null, viewModel, organisationRole, ct);

        if (result is OrganisationRoleChangeResult.NotFound) return NotFound();
        if (result is OrganisationRoleChangeResult.ValidationError e)
        {
            ModelState.AddModelError(e.ModelKey, e.Message);
            return View(nameof(ChangeRole), await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, organisationRole, ct));
        }

        return RedirectToAction(nameof(ChangeRoleCheck), new { id, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change/check")]
    public async Task<IActionResult> ChangeRoleCheck(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(id, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeRole), new { id, cdpPersonId });

        return View(nameof(ChangeRoleCheck), organisationRoleFlowService.StateToViewModel(state));
    }

    [HttpPost("user/{cdpPersonId:guid}/organisation-role/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRoleCheckSubmit(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(id, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeRole), new { id, cdpPersonId });

        var currentViewModel = await organisationRoleFlowService.GetUserViewModelAsync(id, cdpPersonId, ct);
        if (currentViewModel is not null && currentViewModel.CurrentRole == state.SelectedRole)
        {
            return RedirectToAction(nameof(ChangeRoleSuccess), new { id, cdpPersonId });
        }

        var success = await organisationRoleFlowService.UpdateUserRoleAsync(
            id, cdpPersonId, state.SelectedRole, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeRoleSuccess), new { id, cdpPersonId })!);
    }

    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change/success")]
    public async Task<IActionResult> ChangeRoleSuccess(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetSuccessViewModelAsync(
            id, cdpPersonId, null, ct);
        if (viewModel is null) return RedirectToAction(nameof(ChangeRole), new { id, cdpPersonId });

        return View(nameof(ChangeRoleSuccess), viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change")]
    public async Task<IActionResult> ChangeInviteRole(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetInviteViewModelAsync(id, inviteGuid, ct);
        if (viewModel is null) return NotFound();

        var state = await organisationRoleFlowService.GetOrCreateStateAsync(id, null, inviteGuid, viewModel, ct);
        return View(nameof(ChangeRole), await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, state.SelectedRole, ct));
    }

    [HttpPost("invites/{inviteGuid:guid}/organisation-role/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteRoleSubmit(
        Guid id, Guid inviteGuid, OrganisationRole? organisationRole, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetInviteViewModelAsync(id, inviteGuid, ct);
        if (viewModel is null) return NotFound();

        var result = await organisationRoleFlowService.ValidateAndSaveRoleChangeAsync(
            id, null, inviteGuid, viewModel, organisationRole, ct);

        if (result is OrganisationRoleChangeResult.NotFound) return NotFound();
        if (result is OrganisationRoleChangeResult.ValidationError e)
        {
            ModelState.AddModelError(e.ModelKey, e.Message);
            return View(nameof(ChangeRole), await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, organisationRole, ct));
        }

        return RedirectToAction(nameof(ChangeInviteRoleCheck), new { id, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change/check")]
    public async Task<IActionResult> ChangeInviteRoleCheck(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(id, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteRole), new { id, inviteGuid });

        return View(nameof(ChangeRoleCheck), organisationRoleFlowService.StateToViewModel(state));
    }

    [HttpPost("invites/{inviteGuid:guid}/organisation-role/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteRoleCheckSubmit(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(id, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteRole), new { id, inviteGuid });

        var currentViewModel = await organisationRoleFlowService.GetInviteViewModelAsync(id, inviteGuid, ct);
        if (currentViewModel is not null && currentViewModel.CurrentRole == state.SelectedRole)
        {
            return RedirectToAction(nameof(ChangeInviteRoleSuccess), new { id, inviteGuid });
        }

        var success = await organisationRoleFlowService.UpdateInviteRoleAsync(
            id, inviteGuid, state.SelectedRole, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeInviteRoleSuccess), new { id, inviteGuid })!);
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change/success")]
    public async Task<IActionResult> ChangeInviteRoleSuccess(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetSuccessViewModelAsync(
            id, null, inviteGuid, ct);
        if (viewModel is null) return RedirectToAction(nameof(ChangeInviteRole), new { id, inviteGuid });

        return View(nameof(ChangeRoleSuccess), viewModel);
    }
}
