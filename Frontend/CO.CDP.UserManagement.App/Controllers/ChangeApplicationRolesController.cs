using CO.CDP.UserManagement.App.Application.ApplicationRoles;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class ChangeApplicationRolesController(
    IApplicationRoleFlowService applicationRoleFlowService) : UsersBaseController
{
    [HttpGet("user/{cdpPersonId:guid}/application-roles/change")]
    public async Task<IActionResult> ChangeApplicationRoles(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await applicationRoleFlowService.GetUserViewModelWithStateAsync(id, cdpPersonId, ct);
        return viewModel is null ? NotFound() : View(nameof(ChangeApplicationRoles), viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/application-roles/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeApplicationRolesSubmit(
        Guid id, Guid cdpPersonId, ApplicationRoleChangePostModel input, CancellationToken ct)
    {
        var result = await applicationRoleFlowService.ProcessSubmitAsync(
            id, cdpPersonId, null, input, ct);
        if (result is ApplicationRoleSubmitResult.NotFound) return NotFound();
        if (result is ApplicationRoleSubmitResult.ValidationError ve)
        {
            foreach (var (key, msg) in ve.Errors) ModelState.AddModelError(key, msg);
            return View(nameof(ChangeApplicationRoles), ve.ViewModel);
        }
        return RedirectToAction(nameof(ChangeApplicationRolesCheck), new { id, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/application-roles/change/check")]
    public async Task<IActionResult> ChangeApplicationRolesCheck(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(id, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { id, cdpPersonId });

        return View(nameof(ChangeApplicationRolesCheck), applicationRoleFlowService.BuildCheckViewModel(state));
    }

    [HttpPost("user/{cdpPersonId:guid}/application-roles/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeApplicationRolesCheckSubmit(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(id, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { id, cdpPersonId });

        var assignments = applicationRoleFlowService.BuildAssignments(state);
        var success = await applicationRoleFlowService.UpdateUserRolesAsync(id, cdpPersonId, assignments, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeApplicationRolesSuccess), new { id, cdpPersonId })!);
    }

    [HttpGet("user/{cdpPersonId:guid}/application-roles/change/success")]
    public async Task<IActionResult> ChangeApplicationRolesSuccess(
        Guid id, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(id, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { id, cdpPersonId });

        await applicationRoleFlowService.ClearStateAsync(ct);

        var successVm = applicationRoleFlowService.BuildSuccessViewModel(id, state);
        if (successVm is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { id, cdpPersonId });

        return View(nameof(ChangeApplicationRolesSuccess), successVm);
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change")]
    public async Task<IActionResult> ChangeInviteApplicationRoles(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await applicationRoleFlowService.GetInviteViewModelWithStateAsync(id, inviteGuid, ct);
        return viewModel is null ? NotFound() : View(nameof(ChangeApplicationRoles), viewModel);
    }

    [HttpPost("invites/{inviteGuid:guid}/application-roles/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteApplicationRolesSubmit(
        Guid id, Guid inviteGuid, ApplicationRoleChangePostModel input, CancellationToken ct)
    {
        var result = await applicationRoleFlowService.ProcessSubmitAsync(
            id, null, inviteGuid, input, ct);
        if (result is ApplicationRoleSubmitResult.NotFound) return NotFound();
        if (result is ApplicationRoleSubmitResult.ValidationError ve)
        {
            foreach (var (key, msg) in ve.Errors) ModelState.AddModelError(key, msg);
            return View(nameof(ChangeApplicationRoles), ve.ViewModel);
        }
        return RedirectToAction(nameof(ChangeInviteApplicationRolesCheck), new { id, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change/check")]
    public async Task<IActionResult> ChangeInviteApplicationRolesCheck(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(id, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { id, inviteGuid });

        return View(nameof(ChangeApplicationRolesCheck), applicationRoleFlowService.BuildCheckViewModel(state));
    }

    [HttpPost("invites/{inviteGuid:guid}/application-roles/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteApplicationRolesCheckSubmit(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(id, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { id, inviteGuid });

        var assignments = applicationRoleFlowService.BuildAssignments(state);
        var success = await applicationRoleFlowService.UpdateInviteRolesAsync(id, inviteGuid, assignments, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeInviteApplicationRolesSuccess), new { id, inviteGuid })!);
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change/success")]
    public async Task<IActionResult> ChangeInviteApplicationRolesSuccess(
        Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(id, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { id, inviteGuid });

        await applicationRoleFlowService.ClearStateAsync(ct);

        var successVm = applicationRoleFlowService.BuildSuccessViewModel(id, state);
        if (successVm is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { id, inviteGuid });

        return View(nameof(ChangeApplicationRolesSuccess), successVm);
    }
}
