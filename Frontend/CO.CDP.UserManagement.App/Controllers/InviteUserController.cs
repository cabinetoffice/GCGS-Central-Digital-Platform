using CO.CDP.UserManagement.App.Application.InviteUsers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class InviteUserController(
    IInviteUserFlowService inviteUserFlowService,
    IInviteUserStateStore inviteUserStateStore,
    IApplicationRoleSelectionMapper roleSelectionMapper) : UsersBaseController
{
    [HttpGet("add-user")]
    public async Task<IActionResult> Add(string organisationSlug, bool returnToCheckAnswers = false,
        CancellationToken ct = default)
    {
        var existingState = await inviteUserStateStore.GetAsync();
        if (existingState is not null &&
            !existingState.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            await inviteUserStateStore.ClearAsync();
            existingState = null;
        }

        var input = existingState is null
            ? null
            : new InviteUserViewModel
            {
                OrganisationSlug = existingState.OrganisationSlug,
                Email = existingState.Email,
                FirstName = existingState.FirstName,
                LastName = existingState.LastName,
                OrganisationRole = existingState.OrganisationRole
            };
        ViewData["ReturnToCheckAnswers"] = returnToCheckAnswers;
        var viewModel = await inviteUserFlowService.GetViewModelAsync(organisationSlug, input, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost("add-user")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        string organisationSlug,
        InviteUserViewModel input,
        bool returnToCheckAnswers = false,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnToCheckAnswers"] = returnToCheckAnswers;
            var viewModel = await inviteUserFlowService.GetViewModelAsync(organisationSlug, input, ct);
            return viewModel is null ? NotFound() : View(viewModel);
        }

        if (await inviteUserFlowService.IsEmailAlreadyInOrganisationAsync(organisationSlug, input.Email!, ct))
        {
            ModelState.AddModelError(nameof(input.Email),
                "A user with this email address is already in this organisation");
            ViewData["ReturnToCheckAnswers"] = returnToCheckAnswers;
            var viewModel = await inviteUserFlowService.GetViewModelAsync(organisationSlug, input, ct);
            return viewModel is null ? NotFound() : View(viewModel);
        }

        var existingState = await inviteUserStateStore.GetAsync();
        var canPreserveSelections = existingState is not null &&
                                    existingState.OrganisationSlug.Equals(organisationSlug,
                                        StringComparison.OrdinalIgnoreCase);
        var preservedState = canPreserveSelections ? existingState : null;
        var state = new InviteUserState(
            organisationSlug,
            input.Email ?? string.Empty,
            input.FirstName ?? string.Empty,
            input.LastName ?? string.Empty,
            preservedState?.OrganisationRole ?? OrganisationRole.Member,
            preservedState?.ApplicationAssignments);
        await inviteUserStateStore.SetAsync(state);

        if (returnToCheckAnswers)
        {
            return RedirectToAction(nameof(CheckAnswersStep), new { organisationSlug });
        }

        return RedirectToAction(nameof(OrganisationRoleStep), new { organisationSlug });
    }

    [HttpGet("add-user/organisation-role")]
    public async Task<IActionResult> OrganisationRoleStep(
        string organisationSlug,
        bool returnToCheckAnswers = false,
        CancellationToken ct = default)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Add), new { organisationSlug });
        }

        return View(nameof(OrganisationRoleStep),
            await inviteUserFlowService.GetOrganisationRoleStepViewModelAsync(state, returnToCheckAnswers, ct));
    }

    [HttpGet("add-user/application-roles")]
    public async Task<IActionResult> ApplicationRolesStep(string organisationSlug, OrganisationRole? organisationRole,
        CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Add), new { organisationSlug });

        var viewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
        if (viewModel is null) return NotFound();

        return View(nameof(ApplicationRolesStep), roleSelectionMapper.ApplyExistingSelections(viewModel, state.ApplicationAssignments));
    }

    [HttpPost("add-user/application-roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplicationRolesStepSubmit(
        string organisationSlug,
        ApplicationRolesStepPostModel input,
        CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Add), new { organisationSlug });

        var serverViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
        if (serverViewModel is null) return NotFound();

        var merged = roleSelectionMapper.MergePostedSelections(serverViewModel, input);

        if (!roleSelectionMapper.ValidateSelections(merged, ModelState))
            return View(nameof(ApplicationRolesStep), merged);

        var assignments = roleSelectionMapper.MapToAssignments(
            merged.Applications.Where(a => a.GiveAccess).ToList());

        state = state with { ApplicationAssignments = assignments };
        await inviteUserStateStore.SetAsync(state);
        return RedirectToAction(nameof(CheckAnswersStep), new { organisationSlug });
    }

    [HttpGet("add-user/check-answers")]
    public async Task<IActionResult> CheckAnswersStep(
        string organisationSlug,
        OrganisationRole? organisationRole,
        CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Add), new { organisationSlug });
        }

        var selectedAssignments = state.ApplicationAssignments ?? [];
        if (selectedAssignments.Count == 0)
        {
            return RedirectToAction(nameof(ApplicationRolesStep), new { organisationSlug });
        }

        var rolesViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
        if (rolesViewModel is null) return NotFound();

        var applications = selectedAssignments
            .Select(assignment =>
            {
                var app = rolesViewModel.Applications.FirstOrDefault(a =>
                    a.OrganisationApplicationId == assignment.OrganisationApplicationId);
                var role = app?.Roles.FirstOrDefault(r => r.Id == assignment.ApplicationRoleId);
                return app is null || role is null
                    ? null
                    : new InviteCheckAnswersApplicationViewModel
                    {
                        ApplicationName = app.ApplicationName,
                        RoleName = role.Name
                    };
            })
            .Where(item => item is not null)
            .Cast<InviteCheckAnswersApplicationViewModel>()
            .ToList();

        if (applications.Count == 0)
        {
            return RedirectToAction(nameof(ApplicationRolesStep), new { organisationSlug });
        }

        return View(nameof(CheckAnswersStep), new InviteCheckAnswersViewModel
        {
            OrganisationSlug = state.OrganisationSlug,
            FirstName = state.FirstName,
            LastName = state.LastName,
            Email = state.Email,
            OrganisationRole = state.OrganisationRole,
            Applications = applications
        });
    }

    [HttpPost("add-user/check-answers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckAnswersStepSubmit(string organisationSlug, CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Add), new { organisationSlug });
        }

        var assignments = state.ApplicationAssignments ?? [];
        if (assignments.Count == 0)
        {
            return RedirectToAction(nameof(ApplicationRolesStep), new { organisationSlug });
        }

        var rolesViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
        if (rolesViewModel is null) return NotFound();

        var applicationRoles = assignments
            .Select(assignment =>
            {
                var app = rolesViewModel.Applications.FirstOrDefault(a =>
                    a.OrganisationApplicationId == assignment.OrganisationApplicationId);
                var role = app?.Roles.FirstOrDefault(r => r.Id == assignment.ApplicationRoleId);
                return app is null || role is null
                    ? null
                    : new InviteSuccessApplicationRoleViewModel
                    {
                        ApplicationName = app.ApplicationName,
                        RoleName = role.Name
                    };
            })
            .Where(item => item is not null)
            .Cast<InviteSuccessApplicationRoleViewModel>()
            .ToList();

        if (applicationRoles.Count == 0)
        {
            return RedirectToAction(nameof(ApplicationRolesStep), new { organisationSlug });
        }

        var invitePageModel = await inviteUserFlowService.GetViewModelAsync(organisationSlug, null, ct);
        if (invitePageModel is null) return NotFound();

        var success = await inviteUserFlowService.InviteAsync(organisationSlug, state, ct);
        if (success.IsFailure) return Redirect("/error");
        if (success.Match(_ => false, outcome => outcome == CO.CDP.UserManagement.App.Services.ServiceOutcome.NotFound))
            return NotFound();

        await inviteUserStateStore.SetSuccessAsync(new InviteSuccessState
        {
            OrganisationSlug = state.OrganisationSlug,
            OrganisationName = invitePageModel.OrganisationName,
            FirstName = state.FirstName,
            LastName = state.LastName,
            Email = state.Email,
            OrganisationRole = state.OrganisationRole,
            DateAdded = DateTimeOffset.UtcNow,
            Applications = applicationRoles
        });

        await inviteUserStateStore.ClearAsync();
        return RedirectToAction(nameof(InviteSuccessStep), new { organisationSlug });
    }

    [HttpGet("add-user/success")]
    public async Task<IActionResult> InviteSuccessStep(string organisationSlug)
    {
        var successState = await inviteUserStateStore.GetSuccessAsync();
        if (successState is null ||
            !successState.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug });
        }

        return View(nameof(InviteSuccessStep), new InviteSuccessViewModel
        {
            OrganisationSlug = successState.OrganisationSlug,
            OrganisationName = successState.OrganisationName,
            FirstName = successState.FirstName,
            LastName = successState.LastName,
            Email = successState.Email,
            OrganisationRole = successState.OrganisationRole,
            DateAdded = successState.DateAdded,
            Applications = successState.Applications
        });
    }

    [HttpGet("invites/{inviteGuid:guid}/resend-invite")]
    public async Task<IActionResult> ResendInvite(string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var success = await inviteUserFlowService.ResendInviteAsync(organisationSlug, inviteGuid, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == CO.CDP.UserManagement.App.Services.ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(UsersListController.Index), "UsersList", new { organisationSlug })!);
    }

}
