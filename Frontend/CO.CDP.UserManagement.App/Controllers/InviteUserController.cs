using CO.CDP.UserManagement.App.Application.InviteUsers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class InviteUserController(
    IInviteUserFlowService inviteUserFlowService,
    IInviteUserStateStore inviteUserStateStore,
    IApplicationRoleSelectionMapper roleSelectionMapper,
    ILogger<InviteUserController> logger) : UsersBaseController
{
    [HttpGet("add-user")]
    public async Task<IActionResult> Add(Guid id, bool returnToCheckAnswers = false,
        CancellationToken ct = default)
    {
        var existingState = await inviteUserStateStore.GetAsync();
        if (existingState is not null && existingState.OrganisationId != id)
        {
            await inviteUserStateStore.ClearAsync();
            existingState = null;
        }

        var input = existingState is null
            ? null
            : new InviteUserViewModel
            {
                OrganisationId = existingState.OrganisationId,
                Email = existingState.Email,
                FirstName = existingState.FirstName,
                LastName = existingState.LastName,
                OrganisationRole = existingState.OrganisationRole
            };
        ViewData["ReturnToCheckAnswers"] = returnToCheckAnswers;
        var viewModel = await inviteUserFlowService.GetViewModelAsync(id, input, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost("add-user")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        Guid id,
        InviteUserViewModel input,
        bool returnToCheckAnswers = false,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnToCheckAnswers"] = returnToCheckAnswers;
            var viewModel = await inviteUserFlowService.GetViewModelAsync(id, input, ct);
            return viewModel is null ? NotFound() : View(viewModel);
        }

        if (await inviteUserFlowService.IsEmailAlreadyInOrganisationAsync(id, input.Email!, ct))
        {
            ModelState.AddModelError(nameof(input.Email),
                "A user with this email address is already in this organisation");
            ViewData["ReturnToCheckAnswers"] = returnToCheckAnswers;
            var viewModel = await inviteUserFlowService.GetViewModelAsync(id, input, ct);
            return viewModel is null ? NotFound() : View(viewModel);
        }

        var existingState = await inviteUserStateStore.GetAsync();
        var canPreserveSelections = existingState is not null && existingState.OrganisationId == id;
        var preservedState = canPreserveSelections ? existingState : null;
        var state = new InviteUserState(
            id,
            input.Email ?? string.Empty,
            input.FirstName ?? string.Empty,
            input.LastName ?? string.Empty,
            preservedState?.OrganisationRole ?? OrganisationRole.Member,
            preservedState?.ApplicationAssignments);
        await inviteUserStateStore.SetAsync(state);

        if (returnToCheckAnswers)
        {
            return RedirectToAction(nameof(CheckAnswersStep), new { id });
        }

        return RedirectToAction(nameof(OrganisationRoleStep), new { id });
    }

    [HttpGet("add-user/organisation-role")]
    public async Task<IActionResult> OrganisationRoleStep(
        Guid id,
        bool returnToCheckAnswers = false,
        CancellationToken ct = default)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || state.OrganisationId != id)
        {
            return RedirectToAction(nameof(Add), new { id });
        }

        return View(nameof(OrganisationRoleStep),
            await inviteUserFlowService.GetOrganisationRoleStepViewModelAsync(state, returnToCheckAnswers, ct));
    }

    [HttpGet("add-user/application-roles")]
    public async Task<IActionResult> ApplicationRolesStep(Guid id, OrganisationRole? organisationRole,
        CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || state.OrganisationId != id)
            return RedirectToAction(nameof(Add), new { id });

        var viewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(id, state, ct);
        if (viewModel is null) return NotFound();

        return View(nameof(ApplicationRolesStep),
            roleSelectionMapper.ApplyExistingSelections(viewModel, state.ApplicationAssignments));
    }

    [HttpPost("add-user/application-roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplicationRolesStepSubmit(
        Guid id,
        ApplicationRolesStepPostModel input,
        CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || state.OrganisationId != id)
            return RedirectToAction(nameof(Add), new { id });

        var serverViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(id, state, ct);
        if (serverViewModel is null) return NotFound();

        var merged = roleSelectionMapper.MergePostedSelections(serverViewModel, input);

        if (!roleSelectionMapper.ValidateSelections(merged, ModelState))
            return View(nameof(ApplicationRolesStep), merged);

        var assignments = roleSelectionMapper.MapToAssignments(
            merged.Applications.Where(a => a.GiveAccess).ToList());

        state = state with { ApplicationAssignments = assignments };
        await inviteUserStateStore.SetAsync(state);
        return RedirectToAction(nameof(CheckAnswersStep), new { id });
    }

    [HttpGet("add-user/check-answers")]
    public async Task<IActionResult> CheckAnswersStep(
        Guid id,
        OrganisationRole? organisationRole,
        CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || state.OrganisationId != id)
        {
            return RedirectToAction(nameof(Add), new { id });
        }

        var selectedAssignments = state.ApplicationAssignments ?? [];
        if (selectedAssignments.Count == 0)
        {
            return RedirectToAction(nameof(ApplicationRolesStep), new { id });
        }

        var rolesViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(id, state, ct);
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
            return RedirectToAction(nameof(ApplicationRolesStep), new { id });
        }

        return View(nameof(CheckAnswersStep), new InviteCheckAnswersViewModel
        {
            OrganisationId = state.OrganisationId,
            FirstName = state.FirstName,
            LastName = state.LastName,
            Email = state.Email,
            OrganisationRole = state.OrganisationRole,
            Applications = applications
        });
    }

    [HttpPost("add-user/check-answers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckAnswersStepSubmit(Guid id, CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || state.OrganisationId != id)
        {
            return RedirectToAction(nameof(Add), new { id });
        }

        var assignments = state.ApplicationAssignments ?? [];
        if (assignments.Count == 0)
        {
            return RedirectToAction(nameof(ApplicationRolesStep), new { id });
        }

        var rolesViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(id, state, ct);
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
            return RedirectToAction(nameof(ApplicationRolesStep), new { id });
        }

        var invitePageModel = await inviteUserFlowService.GetViewModelAsync(id, null, ct);
        if (invitePageModel is null) return NotFound();

        var success = await inviteUserFlowService.InviteAsync(id, state, ct);
        if (success.IsFailure) return Redirect("/error");
        if (success.Match(_ => false, outcome => outcome == ServiceOutcome.NotFound))
            return NotFound();

        await inviteUserStateStore.SetSuccessAsync(new InviteSuccessState
        {
            OrganisationId = state.OrganisationId,
            OrganisationName = invitePageModel.OrganisationName,
            FirstName = state.FirstName,
            LastName = state.LastName,
            Email = state.Email,
            OrganisationRole = state.OrganisationRole,
            DateAdded = DateTimeOffset.UtcNow,
            Applications = applicationRoles
        });

        await inviteUserStateStore.ClearAsync();
        return RedirectToAction(nameof(InviteSuccessStep), new { id });
    }

    [HttpGet("add-user/success")]
    public async Task<IActionResult> InviteSuccessStep(Guid id)
    {
        var successState = await inviteUserStateStore.GetSuccessAsync();
        if (successState is null || successState.OrganisationId != id)
        {
            return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
        }

        return View(nameof(InviteSuccessStep), new InviteSuccessViewModel
        {
            OrganisationId = successState.OrganisationId,
            OrganisationName = successState.OrganisationName,
            FirstName = successState.FirstName,
            LastName = successState.LastName,
            Email = successState.Email,
            OrganisationRole = successState.OrganisationRole,
            DateAdded = successState.DateAdded,
            Applications = successState.Applications
        });
    }

    [HttpPost("invites/{inviteGuid:guid}/resend-invite")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendInvite(Guid id, Guid inviteGuid, CancellationToken ct)
    {
        var cooldown = TimeSpan.FromMinutes(1);

        if (!ResendCooldown.IsAllowed(HttpContext.Session, inviteGuid, cooldown))
        {
            logger.LogInformation(
                "Invite resend blocked by cooldown. OrganisationId={OrganisationId} InviteGuid={InviteGuid}", id,
                inviteGuid);
            TempData["InfoBanner"] = "Invite already resent. Please wait a minute before resending again.";
            return RedirectToAction(nameof(UserDetailsController.InviteDetails), "UserDetails", new { id, inviteGuid });
        }

        var result = await inviteUserFlowService.ResendInviteAsync(id, inviteGuid, ct);
        return await result.MatchAsync<IActionResult>(
            _ => Task.FromResult<IActionResult>(Redirect("/error")),
            async outcome =>
            {
                if (outcome.Outcome == ServiceOutcome.NotFound)
                    return NotFound();

                ResendCooldown.Record(HttpContext.Session, inviteGuid);
                logger.LogInformation(
                    "Invite resent successfully. OrganisationId={OrganisationId} InviteGuid={InviteGuid}", id,
                    inviteGuid);

                var name = string.IsNullOrWhiteSpace(outcome.InviteeName) ? "the user" : outcome.InviteeName;
                TempData["SuccessBanner"] = $"Invite resent to {name}.";
                return await Task.FromResult<IActionResult>(
                    RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id }));
            });
    }
}