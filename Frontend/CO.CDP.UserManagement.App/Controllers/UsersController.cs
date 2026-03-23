using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize]
[Route("organisation/{organisationSlug}")]
public class UsersController(
    IUserService userService,
    IInviteUserStateStore inviteUserStateStore,
    IChangeRoleStateStore changeRoleStateStore) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? organisationSlug,
        string? role,
        string? application,
        string? search,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrEmpty(organisationSlug))
        {
            return NotFound();
        }

        var viewModel = await userService.GetUsersViewModelAsync(organisationSlug, role, application, search, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet("add-user")]
    public async Task<IActionResult> Add(string organisationSlug, bool returnToCheckAnswers = false, CancellationToken ct = default)
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
        var viewModel = await userService.GetInviteUserViewModelAsync(organisationSlug, input, ct);
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
            var viewModel = await userService.GetInviteUserViewModelAsync(organisationSlug, input, ct);
            return viewModel is null ? NotFound() : View(viewModel);
        }

        var existingState = await inviteUserStateStore.GetAsync();
        var canPreserveSelections = existingState is not null &&
            existingState.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase);
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
        bool returnToCheckAnswers = false)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Add), new { organisationSlug });
        }

        ViewData["ReturnToCheckAnswers"] = returnToCheckAnswers;
        return View("OrganisationRole", state);
    }

    [HttpGet("add-user/application-roles")]
    public async Task<IActionResult> ApplicationRolesStep(string organisationSlug, OrganisationRole? organisationRole, CancellationToken ct)
    {
        var state = await inviteUserStateStore.GetAsync();
        if (state is null || !state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Add), new { organisationSlug });
        }

        if (organisationRole.HasValue && state.OrganisationRole != organisationRole.Value)
        {
            state = state with { OrganisationRole = organisationRole.Value };
            await inviteUserStateStore.SetAsync(state);
        }

        var viewModel = await userService.GetApplicationRolesStepViewModelAsync(organisationSlug, state, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var selectedAssignments = (state.ApplicationAssignments ?? [])
            .ToDictionary(a => a.OrganisationApplicationId, a => a.ApplicationRoleId);
        viewModel.Applications
            .Where(application => selectedAssignments.ContainsKey(application.OrganisationApplicationId))
            .ToList()
            .ForEach(application =>
            {
                application.GiveAccess = true;
                application.SelectedRoleId = selectedAssignments[application.OrganisationApplicationId];
            });

        return View("ApplicationRoles", viewModel);
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
        {
            return RedirectToAction(nameof(Add), new { organisationSlug });
        }

        var viewModel = await userService.GetApplicationRolesStepViewModelAsync(organisationSlug, state, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var postedSelections = input.Applications.ToDictionary(a => a.OrganisationApplicationId);
        viewModel.Applications
            .Where(application => postedSelections.ContainsKey(application.OrganisationApplicationId))
            .ToList()
            .ForEach(application =>
            {
                var postedSelection = postedSelections[application.OrganisationApplicationId];
                application.GiveAccess = postedSelection.GiveAccess;
                application.SelectedRoleId = postedSelection.SelectedRoleId;
            });

        var selectedApplications = viewModel.Applications.Where(a => a.GiveAccess).ToList();
        if (selectedApplications.Count == 0)
        {
            ModelState.AddModelError("applicationSelections", "You must select at least one application and role");
        }

        viewModel.Applications
            .Select((application, index) => new { application, index })
            .Where(item => item.application.GiveAccess &&
                           (!item.application.SelectedRoleId.HasValue ||
                            item.application.Roles.All(role => role.Id != item.application.SelectedRoleId.Value)))
            .ToList()
            .ForEach(item =>
                ModelState.AddModelError(
                    $"Applications[{item.index}].SelectedRoleId",
                    $"Select a role for {item.application.ApplicationName}"));

        if (!ModelState.IsValid)
        {
            return View("ApplicationRoles", viewModel);
        }

        var assignments = selectedApplications
            .Where(a => a.SelectedRoleId is not null)
            .Select(a => new InviteApplicationAssignment
            {
                OrganisationApplicationId = a.OrganisationApplicationId,
                ApplicationRoleId = a.SelectedRoleId.GetValueOrDefault()
            })
            .ToList();

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

        if (organisationRole.HasValue && state.OrganisationRole != organisationRole.Value)
        {
            state = state with { OrganisationRole = organisationRole.Value };
            await inviteUserStateStore.SetAsync(state);
        }

        var selectedAssignments = state.ApplicationAssignments ?? [];
        if (selectedAssignments.Count == 0)
        {
            return RedirectToAction(nameof(ApplicationRolesStep), new { organisationSlug });
        }

        var rolesViewModel = await userService.GetApplicationRolesStepViewModelAsync(organisationSlug, state, ct);
        if (rolesViewModel is null)
        {
            return NotFound();
        }

        var applications = selectedAssignments
            .Select(assignment =>
            {
                var app = rolesViewModel.Applications.FirstOrDefault(a => a.OrganisationApplicationId == assignment.OrganisationApplicationId);
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

        var viewModel = new InviteCheckAnswersViewModel
        {
            OrganisationSlug = state.OrganisationSlug,
            FirstName = state.FirstName,
            LastName = state.LastName,
            Email = state.Email,
            OrganisationRole = state.OrganisationRole,
            Applications = applications
        };

        return View("CheckAnswers", viewModel);
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

        var rolesViewModel = await userService.GetApplicationRolesStepViewModelAsync(organisationSlug, state, ct);
        if (rolesViewModel is null)
        {
            return NotFound();
        }

        var applicationRoles = assignments
            .Select(assignment =>
            {
                var app = rolesViewModel.Applications.FirstOrDefault(a => a.OrganisationApplicationId == assignment.OrganisationApplicationId);
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

        var invitePageModel = await userService.GetInviteUserViewModelAsync(organisationSlug, ct: ct);
        if (invitePageModel is null)
        {
            return NotFound();
        }

        var inviteInput = new InviteUserViewModel
        {
            OrganisationSlug = state.OrganisationSlug,
            Email = state.Email,
            FirstName = state.FirstName,
            LastName = state.LastName,
            OrganisationRole = state.OrganisationRole
        };
        var success = await userService.InviteUserAsync(
            organisationSlug,
            inviteInput,
            ct,
            assignments);
        if (!success)
        {
            return NotFound();
        }

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
            return RedirectToAction(nameof(Index), new { organisationSlug });
        }

        var viewModel = new InviteSuccessViewModel
        {
            OrganisationSlug = successState.OrganisationSlug,
            OrganisationName = successState.OrganisationName,
            FirstName = successState.FirstName,
            LastName = successState.LastName,
            Email = successState.Email,
            OrganisationRole = successState.OrganisationRole,
            DateAdded = successState.DateAdded,
            Applications = successState.Applications
        };

        return View("InviteSuccess", viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}/resend-invite")]
    public async Task<IActionResult> ResendInvite(string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var success = await userService.ResendInviteAsync(organisationSlug, inviteGuid, ct);
        return success ? RedirectToAction(nameof(Index), new { organisationSlug }) : NotFound();
    }

    [HttpGet("user/{cdpPersonId:guid}/assignments/{assignmentId:int}/revoke")]
    public async Task<IActionResult> RevokeApplicationAccess(
        string organisationSlug,
        Guid cdpPersonId,
        int assignmentId,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(organisationSlug))
            return NotFound();

        var viewModel = await userService.GetRevokeApplicationAccessViewModelAsync(organisationSlug, cdpPersonId, assignmentId, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/assignments/{assignmentId:int}/revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeApplicationAccess(
        string organisationSlug,
        Guid cdpPersonId,
        int assignmentId,
        bool? confirmRevoke,
        CancellationToken ct)
    {
        if (confirmRevoke == null)
        {
            ModelState.AddModelError(nameof(confirmRevoke), "Select if you want to revoke access");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userService.GetRevokeApplicationAccessViewModelAsync(organisationSlug, cdpPersonId, assignmentId, ct);
            return viewModel is null ? NotFound() : View(viewModel);
        }

        // "No, keep access" — redirect back without calling the API
        if (confirmRevoke == false)
        {
            return RedirectToAction(nameof(Index), new { organisationSlug });
        }

        // "Yes, revoke access" — call the API
        var revokeViewModel = await userService.GetRevokeApplicationAccessViewModelAsync(organisationSlug, cdpPersonId, assignmentId, ct);
        if (revokeViewModel is null)
            return NotFound();

        var success = await userService.RevokeApplicationAccessAsync(
            organisationSlug,
            revokeViewModel.UserPrincipalId,
            revokeViewModel.OrgId,
            assignmentId,
            ct);

        if (!success)
            return RedirectToAction("Error", "Home");

        TempData["RevokeSuccessUserName"] = revokeViewModel.UserDisplayName;
        TempData["RevokeSuccessApplicationName"] = revokeViewModel.ApplicationName;

        return RedirectToAction(nameof(RevokeApplicationAccessSuccess), new { organisationSlug, cdpPersonId, assignmentId });
    }

    [HttpGet("user/{cdpPersonId:guid}/assignments/{assignmentId:int}/revoke/success")]
    public IActionResult RevokeApplicationAccessSuccess(string organisationSlug, Guid cdpPersonId, int assignmentId)
    {
        var userName = TempData["RevokeSuccessUserName"] as string;
        var applicationName = TempData["RevokeSuccessApplicationName"] as string;

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(applicationName))
            return RedirectToAction(nameof(Index), new { organisationSlug });

        return View(new RevokeApplicationAccessSuccessViewModel(
            OrganisationSlug: organisationSlug,
            UserDisplayName: userName,
            ApplicationName: applicationName));
    }

    [HttpGet("user/{cdpPersonId:guid}/change-role")]
    public async Task<IActionResult> ChangeRole(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var viewModel = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, cdpPersonId, null, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var state = await GetOrCreateChangeRoleStateAsync(organisationSlug, cdpPersonId, null, viewModel);
        return View("ChangeRole", viewModel with { SelectedRole = state.SelectedRole });
    }

    [HttpPost("user/{cdpPersonId:guid}/change-role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRoleSubmit(
        string organisationSlug,
        Guid cdpPersonId,
        OrganisationRole? organisationRole,
        CancellationToken ct)
    {
        if (organisationRole == null)
        {
            ModelState.AddModelError(nameof(organisationRole), "Select an organisation role");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, cdpPersonId, null, ct);
            return viewModel is null ? NotFound() : View("ChangeRole", viewModel);
        }

        var viewModelToPersist = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, cdpPersonId, null, ct);
        if (viewModelToPersist is null)
        {
            return NotFound();
        }

        var selectedRole = organisationRole.GetValueOrDefault();
        await changeRoleStateStore.SetAsync(ToChangeRoleState(viewModelToPersist, selectedRole));
        return RedirectToAction(nameof(ChangeRoleCheck), new { organisationSlug, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/change-role/check")]
    public async Task<IActionResult> ChangeRoleCheck(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeRoleStateAsync(organisationSlug, cdpPersonId, null);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeRole), new { organisationSlug, cdpPersonId });
        }

        return View("ChangeRoleCheck", ToChangeRoleViewModel(state));
    }

    [HttpPost("user/{cdpPersonId:guid}/change-role/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRoleCheckSubmit(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeRoleStateAsync(organisationSlug, cdpPersonId, null);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeRole), new { organisationSlug, cdpPersonId });
        }

        var success = await userService.UpdateUserRoleAsync(
            organisationSlug,
            cdpPersonId,
            null,
            state.SelectedRole,
            ct);
        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(ChangeRoleSuccess), new { organisationSlug, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/change-role/success")]
    public async Task<IActionResult> ChangeRoleSuccess(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeRoleStateAsync(organisationSlug, cdpPersonId, null);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeRole), new { organisationSlug, cdpPersonId });
        }

        await changeRoleStateStore.ClearAsync();

        return View("ChangeRoleSuccess", new ChangeUserRoleSuccessViewModel(
            OrganisationSlug: organisationSlug,
            UserDisplayName: state.UserDisplayName,
            NewRole: state.SelectedRole,
            RoleDescription: state.SelectedRole.GetDescription()));
    }

    [HttpGet("invites/{inviteGuid:guid}/change-role")]
    public async Task<IActionResult> ChangeInviteRole(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var viewModel = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, null, inviteGuid, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var state = await GetOrCreateChangeRoleStateAsync(organisationSlug, null, inviteGuid, viewModel);
        return View("ChangeRole", viewModel with { SelectedRole = state.SelectedRole });
    }

    [HttpPost("invites/{inviteGuid:guid}/change-role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteRoleSubmit(
        string organisationSlug,
        Guid inviteGuid,
        OrganisationRole? organisationRole,
        CancellationToken ct)
    {
        if (organisationRole == null)
        {
            ModelState.AddModelError(nameof(organisationRole), "Select an organisation role");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, null, inviteGuid, ct);
            return viewModel is null ? NotFound() : View("ChangeRole", viewModel);
        }

        var viewModelToPersist = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, null, inviteGuid, ct);
        if (viewModelToPersist is null)
        {
            return NotFound();
        }

        var selectedRole = organisationRole.GetValueOrDefault();
        await changeRoleStateStore.SetAsync(ToChangeRoleState(viewModelToPersist, selectedRole));
        return RedirectToAction(nameof(ChangeInviteRoleCheck), new { organisationSlug, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/change-role/check")]
    public async Task<IActionResult> ChangeInviteRoleCheck(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeRoleStateAsync(organisationSlug, null, inviteGuid);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeInviteRole), new { organisationSlug, inviteGuid });
        }

        return View("ChangeRoleCheck", ToChangeRoleViewModel(state));
    }

    [HttpPost("invites/{inviteGuid:guid}/change-role/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteRoleCheckSubmit(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeRoleStateAsync(organisationSlug, null, inviteGuid);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeInviteRole), new { organisationSlug, inviteGuid });
        }

        var success = await userService.UpdateUserRoleAsync(
            organisationSlug,
            null,
            inviteGuid,
            state.SelectedRole,
            ct);
        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(ChangeInviteRoleSuccess), new { organisationSlug, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/change-role/success")]
    public async Task<IActionResult> ChangeInviteRoleSuccess(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeRoleStateAsync(organisationSlug, null, inviteGuid);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeInviteRole), new { organisationSlug, inviteGuid });
        }

        await changeRoleStateStore.ClearAsync();

        return View("ChangeRoleSuccess", new ChangeUserRoleSuccessViewModel(
            OrganisationSlug: organisationSlug,
            UserDisplayName: state.UserDisplayName,
            NewRole: state.SelectedRole,
            RoleDescription: state.SelectedRole.GetDescription()));
    }

    private async Task<ChangeRoleState> GetOrCreateChangeRoleStateAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ChangeUserRoleViewModel viewModel)
    {
        var existingState = await GetValidatedChangeRoleStateAsync(organisationSlug, cdpPersonId, inviteGuid);
        if (existingState is not null)
        {
            return existingState;
        }

        var selectedRole = viewModel.SelectedRole ?? viewModel.CurrentRole;
        var createdState = ToChangeRoleState(viewModel, selectedRole);
        await changeRoleStateStore.SetAsync(createdState);
        return createdState;
    }

    private async Task<ChangeRoleState?> GetValidatedChangeRoleStateAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid)
    {
        var state = await changeRoleStateStore.GetAsync();
        if (state is null)
        {
            return null;
        }

        if (!state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase) ||
            state.CdpPersonId != cdpPersonId ||
            state.InviteGuid != inviteGuid)
        {
            await changeRoleStateStore.ClearAsync();
            return null;
        }

        return state;
    }

    private static ChangeRoleState ToChangeRoleState(ChangeUserRoleViewModel viewModel, OrganisationRole selectedRole) =>
        new(
            viewModel.OrganisationSlug,
            viewModel.CdpPersonId,
            viewModel.InviteGuid,
            viewModel.UserDisplayName,
            viewModel.Email,
            viewModel.CurrentRole,
            selectedRole);

    private static ChangeUserRoleViewModel ToChangeRoleViewModel(ChangeRoleState state) =>
        new(
            OrganisationName: string.Empty,
            OrganisationSlug: state.OrganisationSlug,
            UserDisplayName: state.UserDisplayName,
            Email: state.Email,
            CurrentRole: state.CurrentRole,
            SelectedRole: state.SelectedRole,
            IsPending: state.InviteGuid.HasValue,
            CdpPersonId: state.CdpPersonId,
            InviteGuid: state.InviteGuid);
}
