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
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null) return NotFound();

        var state = await organisationRoleFlowService.GetOrCreateStateAsync(organisationSlug, cdpPersonId, null, viewModel, ct);
        return View(await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, state.SelectedRole, ct));
    }

    [HttpPost("user/{cdpPersonId:guid}/organisation-role/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRoleSubmit(
        string organisationSlug, Guid cdpPersonId, OrganisationRole? organisationRole, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null) return NotFound();

        var result = await organisationRoleFlowService.ValidateAndSaveRoleChangeAsync(
            organisationSlug, cdpPersonId, null, viewModel, organisationRole, ct);

        if (result is OrganisationRoleChangeResult.NotFound) return NotFound();
        if (result is OrganisationRoleChangeResult.ValidationError e)
        {
            ModelState.AddModelError(e.ModelKey, e.Message);
            return View(nameof(ChangeRole), await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, organisationRole, ct));
        }

        return RedirectToAction(nameof(ChangeRoleCheck), new { organisationSlug, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change/check")]
    public async Task<IActionResult> ChangeRoleCheck(
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(organisationSlug, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeRole), new { organisationSlug, cdpPersonId });

        return View(organisationRoleFlowService.StateToViewModel(state));
    }

    [HttpPost("user/{cdpPersonId:guid}/organisation-role/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRoleCheckSubmit(
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(organisationSlug, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeRole), new { organisationSlug, cdpPersonId });

        var currentViewModel = await organisationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (currentViewModel is not null && currentViewModel.CurrentRole == state.SelectedRole)
        {
            return RedirectToAction(nameof(ChangeRoleSuccess), new { organisationSlug, cdpPersonId });
        }

        var success = await organisationRoleFlowService.UpdateUserRoleAsync(
            organisationSlug, cdpPersonId, state.SelectedRole, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeRoleSuccess), new { organisationSlug, cdpPersonId })!);
    }

    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change/success")]
    public async Task<IActionResult> ChangeRoleSuccess(
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetSuccessViewModelAsync(
            organisationSlug, cdpPersonId, null, ct);
        if (viewModel is null) return RedirectToAction(nameof(ChangeRole), new { organisationSlug, cdpPersonId });

        return View(viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change")]
    public async Task<IActionResult> ChangeInviteRole(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (viewModel is null) return NotFound();

        var state = await organisationRoleFlowService.GetOrCreateStateAsync(organisationSlug, null, inviteGuid, viewModel, ct);
        return View(nameof(ChangeRole), await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, state.SelectedRole, ct));
    }

    [HttpPost("invites/{inviteGuid:guid}/organisation-role/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteRoleSubmit(
        string organisationSlug, Guid inviteGuid, OrganisationRole? organisationRole, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (viewModel is null) return NotFound();

        var result = await organisationRoleFlowService.ValidateAndSaveRoleChangeAsync(
            organisationSlug, null, inviteGuid, viewModel, organisationRole, ct);

        if (result is OrganisationRoleChangeResult.NotFound) return NotFound();
        if (result is OrganisationRoleChangeResult.ValidationError e)
        {
            ModelState.AddModelError(e.ModelKey, e.Message);
            return View(nameof(ChangeRole), await organisationRoleFlowService.BuildPageViewModelAsync(viewModel, organisationRole, ct));
        }

        return RedirectToAction(nameof(ChangeInviteRoleCheck), new { organisationSlug, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change/check")]
    public async Task<IActionResult> ChangeInviteRoleCheck(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(organisationSlug, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteRole), new { organisationSlug, inviteGuid });

        return View(nameof(ChangeRoleCheck), organisationRoleFlowService.StateToViewModel(state));
    }

    [HttpPost("invites/{inviteGuid:guid}/organisation-role/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteRoleCheckSubmit(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var state = await organisationRoleFlowService.GetValidatedStateAsync(organisationSlug, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteRole), new { organisationSlug, inviteGuid });

        var currentViewModel = await organisationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (currentViewModel is not null && currentViewModel.CurrentRole == state.SelectedRole)
        {
            return RedirectToAction(nameof(ChangeInviteRoleSuccess), new { organisationSlug, inviteGuid });
        }

        var success = await organisationRoleFlowService.UpdateInviteRoleAsync(
            organisationSlug, inviteGuid, state.SelectedRole, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeInviteRoleSuccess), new { organisationSlug, inviteGuid })!);
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change/success")]
    public async Task<IActionResult> ChangeInviteRoleSuccess(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetSuccessViewModelAsync(
            organisationSlug, null, inviteGuid, ct);
        if (viewModel is null) return RedirectToAction(nameof(ChangeInviteRole), new { organisationSlug, inviteGuid });

        return View(nameof(ChangeRoleSuccess), viewModel);
    }
}
