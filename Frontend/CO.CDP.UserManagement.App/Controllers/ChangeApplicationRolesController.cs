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
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await applicationRoleFlowService.GetUserViewModelWithStateAsync(organisationSlug, cdpPersonId, ct);
        return viewModel is null ? NotFound() : View("~/Views/Users/ChangeApplicationRoles.cshtml", viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/application-roles/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeApplicationRolesSubmit(
        string organisationSlug, Guid cdpPersonId, ApplicationRoleChangePostModel input, CancellationToken ct)
    {
        var result = await applicationRoleFlowService.ProcessSubmitAsync(
            organisationSlug, cdpPersonId, null, input, ct);
        if (result is ApplicationRoleSubmitResult.NotFound) return NotFound();
        if (result is ApplicationRoleSubmitResult.ValidationError ve)
        {
            foreach (var (key, msg) in ve.Errors) ModelState.AddModelError(key, msg);
            return View("~/Views/Users/ChangeApplicationRoles.cshtml", ve.ViewModel);
        }
        return RedirectToAction(nameof(ChangeApplicationRolesCheck), new { organisationSlug, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/application-roles/change/check")]
    public async Task<IActionResult> ChangeApplicationRolesCheck(
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(organisationSlug, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });

        return View("~/Views/Users/CheckApplicationRoles.cshtml", applicationRoleFlowService.BuildCheckViewModel(state));
    }

    [HttpPost("user/{cdpPersonId:guid}/application-roles/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeApplicationRolesCheckSubmit(
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(organisationSlug, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });

        var assignments = applicationRoleFlowService.BuildAssignments(state);
        var success = await applicationRoleFlowService.UpdateUserRolesAsync(organisationSlug, cdpPersonId, assignments, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeApplicationRolesSuccess), new { organisationSlug, cdpPersonId })!);
    }

    [HttpGet("user/{cdpPersonId:guid}/application-roles/change/success")]
    public async Task<IActionResult> ChangeApplicationRolesSuccess(
        string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(organisationSlug, cdpPersonId, null, ct);
        if (state is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });

        await applicationRoleFlowService.ClearStateAsync(ct);

        var successVm = applicationRoleFlowService.BuildSuccessViewModel(organisationSlug, state);
        if (successVm is null) return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });

        return View("~/Views/Users/ChangeApplicationRolesSuccess.cshtml", successVm);
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change")]
    public async Task<IActionResult> ChangeInviteApplicationRoles(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await applicationRoleFlowService.GetInviteViewModelWithStateAsync(organisationSlug, inviteGuid, ct);
        return viewModel is null ? NotFound() : View("~/Views/Users/ChangeApplicationRoles.cshtml", viewModel);
    }

    [HttpPost("invites/{inviteGuid:guid}/application-roles/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteApplicationRolesSubmit(
        string organisationSlug, Guid inviteGuid, ApplicationRoleChangePostModel input, CancellationToken ct)
    {
        var result = await applicationRoleFlowService.ProcessSubmitAsync(
            organisationSlug, null, inviteGuid, input, ct);
        if (result is ApplicationRoleSubmitResult.NotFound) return NotFound();
        if (result is ApplicationRoleSubmitResult.ValidationError ve)
        {
            foreach (var (key, msg) in ve.Errors) ModelState.AddModelError(key, msg);
            return View("~/Views/Users/ChangeApplicationRoles.cshtml", ve.ViewModel);
        }
        return RedirectToAction(nameof(ChangeInviteApplicationRolesCheck), new { organisationSlug, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change/check")]
    public async Task<IActionResult> ChangeInviteApplicationRolesCheck(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(organisationSlug, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });

        return View("~/Views/Users/CheckApplicationRoles.cshtml", applicationRoleFlowService.BuildCheckViewModel(state));
    }

    [HttpPost("invites/{inviteGuid:guid}/application-roles/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteApplicationRolesCheckSubmit(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(organisationSlug, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });

        var assignments = applicationRoleFlowService.BuildAssignments(state);
        var success = await applicationRoleFlowService.UpdateInviteRolesAsync(organisationSlug, inviteGuid, assignments, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeInviteApplicationRolesSuccess), new { organisationSlug, inviteGuid })!);
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change/success")]
    public async Task<IActionResult> ChangeInviteApplicationRolesSuccess(
        string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var state = await applicationRoleFlowService.GetValidatedStateAsync(organisationSlug, null, inviteGuid, ct);
        if (state is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });

        await applicationRoleFlowService.ClearStateAsync(ct);

        var successVm = applicationRoleFlowService.BuildSuccessViewModel(organisationSlug, state);
        if (successVm is null) return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });

        return View("~/Views/Users/ChangeApplicationRolesSuccess.cshtml", successVm);
    }
}
