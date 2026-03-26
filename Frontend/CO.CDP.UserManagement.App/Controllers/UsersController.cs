using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Application.Users;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Application.InviteUsers;
using CO.CDP.UserManagement.App.Application.OrganisationRoles;
using CO.CDP.UserManagement.App.Application.ApplicationRoles;
using CO.CDP.UserManagement.App.Application.Removal;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.App.Attributes;
using CO.CDP.UserManagement.App.Constants;
using CO.CDP.UserManagement.Core;
using CO.CDP.UserManagement.WebApiClient;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize(Policy = PolicyNames.OrganisationOwnerOrAdmin)]
[Route("organisation/{organisationSlug}")]
[OrganisationOwnerOrAdmin]
public class UsersController(
    IUsersQueryService usersQueryService,
    IUserDetailsQueryService userDetailsQueryService,
    IInviteDetailsQueryService inviteDetailsQueryService,
    IInviteUserFlowService inviteUserFlowService,
    IOrganisationRoleFlowService organisationRoleFlowService,
    IApplicationRoleFlowService applicationRoleFlowService,
    IUserRemovalService userRemovalService,
    IOrganisationRoleService organisationRoleService,
    IInviteUserStateStore inviteUserStateStore,
    IChangeRoleStateStore changeRoleStateStore,
    IChangeApplicationRoleStateStore changeApplicationRoleStateStore,
    IUserManagementApiAdapter adapter) : Controller
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

        var viewModel = await usersQueryService.GetViewModelAsync(organisationSlug, role, application, search, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

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

        return View("OrganisationRole", await BuildOrganisationRoleStepViewModelAsync(state, returnToCheckAnswers, ct));
    }

    [HttpGet("add-user/application-roles")]
    public async Task<IActionResult> ApplicationRolesStep(string organisationSlug, OrganisationRole? organisationRole,
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

        var viewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
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

        var viewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
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
                if (application.AllowsMultipleRoleAssignments)
                {
                    application.SelectedRoleIds = postedSelection.SelectedRoleIds;
                    application.SelectedRoleId = postedSelection.SelectedRoleIds.Count > 0
                        ? postedSelection.SelectedRoleIds[0]
                        : null;
                }
                else
                {
                    application.SelectedRoleId = postedSelection.SelectedRoleId;
                }
            });

        var selectedApplications = viewModel.Applications.Where(a => a.GiveAccess).ToList();
        if (selectedApplications.Count == 0)
        {
            ModelState.AddModelError("applicationSelections", "You must select at least one application and role");
        }

        viewModel.Applications
            .Select((application, index) => new { application, index })
            .Where(item => item.application.GiveAccess && !HasRoleSelected(item.application))
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
            .Where(HasRoleSelected)
            .Select(a => new InviteApplicationAssignment
            {
                OrganisationApplicationId = a.OrganisationApplicationId,
                ApplicationRoleId = a.AllowsMultipleRoleAssignments
                    ? (a.SelectedRoleIds.Count > 0 ? a.SelectedRoleIds[0] : 0)
                    : a.SelectedRoleId.GetValueOrDefault(),
                ApplicationRoleIds = a.AllowsMultipleRoleAssignments ? a.SelectedRoleIds : null
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

        var rolesViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
        if (rolesViewModel is null)
        {
            return NotFound();
        }

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

        var rolesViewModel = await inviteUserFlowService.GetApplicationRolesStepAsync(organisationSlug, state, ct);
        if (rolesViewModel is null)
        {
            return NotFound();
        }

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
        var success = await inviteUserFlowService.InviteAsync(
            organisationSlug,
            state,
            ct);
        if (success.IsLeft())
        {
            return Redirect("/error");
        }

        if (success.Match(_ => false, outcome => outcome == ServiceOutcome.NotFound))
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
        var success = await inviteUserFlowService.ResendInviteAsync(organisationSlug, inviteGuid, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(Index), new { organisationSlug })!);
    }

    [HttpGet("invites/{inviteGuid:guid}")]
    public async Task<IActionResult> InviteDetails(string organisationSlug, Guid inviteGuid, CancellationToken ct)
    {
        var viewModel = await inviteDetailsQueryService.GetViewModelAsync(organisationSlug, inviteGuid, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change")]
    public async Task<IActionResult> ChangeRole(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var state = await GetOrCreateChangeRoleStateAsync(organisationSlug, cdpPersonId, null, viewModel);
        return View("ChangeRole", await BuildChangeUserRolePageViewModelAsync(viewModel, state.SelectedRole, ct));
    }

    [HttpPost("user/{cdpPersonId:guid}/organisation-role/change")]
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
            var viewModel = await organisationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
            return viewModel is null
                ? NotFound()
                : View("ChangeRole", await BuildChangeUserRolePageViewModelAsync(viewModel, organisationRole, ct));
        }

        var viewModelToPersist =
            await organisationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModelToPersist is null)
        {
            return NotFound();
        }

        if (IsSelf(viewModelToPersist.Email))
        {
            ModelState.AddModelError(nameof(organisationRole), "You cannot change your own organisation role.");
            return View("ChangeRole",
                await BuildChangeUserRolePageViewModelAsync(viewModelToPersist, organisationRole, ct));
        }

        var selectedRole = organisationRole.GetValueOrDefault();
        if (selectedRole == viewModelToPersist.CurrentRole)
        {
            ModelState.AddModelError(nameof(organisationRole), "Select a different role to continue");
            return View("ChangeRole",
                await BuildChangeUserRolePageViewModelAsync(viewModelToPersist, organisationRole, ct));
        }

        await changeRoleStateStore.SetAsync(ToChangeRoleState(viewModelToPersist, selectedRole));
        return RedirectToAction(nameof(ChangeRoleCheck), new { organisationSlug, cdpPersonId });
    }

    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change/check")]
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

    [HttpPost("user/{cdpPersonId:guid}/organisation-role/change/check")]
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

        // Idempotency: if the role has already been applied, treat as success
        var currentViewModel =
            await organisationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (currentViewModel is not null && currentViewModel.CurrentRole == state.SelectedRole)
        {
            return RedirectToAction(nameof(ChangeRoleSuccess), new { organisationSlug, cdpPersonId });
        }

        var success = await organisationRoleFlowService.UpdateUserRoleAsync(
            organisationSlug,
            cdpPersonId,
            state.SelectedRole,
            ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeRoleSuccess), new { organisationSlug, cdpPersonId })!);
    }

    [HttpGet("user/{cdpPersonId:guid}/organisation-role/change/success")]
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

        var roleDescription = (await organisationRoleService.GetRoleAsync(state.SelectedRole, ct))?.Description ??
                              string.Empty;
        return View("ChangeRoleSuccess", new ChangeUserRoleSuccessViewModel(
            OrganisationSlug: organisationSlug,
            UserDisplayName: state.UserDisplayName,
            NewRole: state.SelectedRole,
            RoleDescription: roleDescription));
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change")]
    public async Task<IActionResult> ChangeInviteRole(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var viewModel = await organisationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var state = await GetOrCreateChangeRoleStateAsync(organisationSlug, null, inviteGuid, viewModel);
        return View("ChangeRole", await BuildChangeUserRolePageViewModelAsync(viewModel, state.SelectedRole, ct));
    }

    [HttpPost("invites/{inviteGuid:guid}/organisation-role/change")]
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
            var viewModel = await organisationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
            return viewModel is null
                ? NotFound()
                : View("ChangeRole", await BuildChangeUserRolePageViewModelAsync(viewModel, organisationRole, ct));
        }

        var viewModelToPersist =
            await organisationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (viewModelToPersist is null)
        {
            return NotFound();
        }

        if (IsSelf(viewModelToPersist.Email))
        {
            ModelState.AddModelError(nameof(organisationRole), "You cannot change your own organisation role.");
            return View("ChangeRole",
                await BuildChangeUserRolePageViewModelAsync(viewModelToPersist, organisationRole, ct));
        }

        var selectedRole = organisationRole.GetValueOrDefault();
        if (selectedRole == viewModelToPersist.CurrentRole)
        {
            ModelState.AddModelError(nameof(organisationRole), "Select a different role to continue");
            return View("ChangeRole",
                await BuildChangeUserRolePageViewModelAsync(viewModelToPersist, organisationRole, ct));
        }

        await changeRoleStateStore.SetAsync(ToChangeRoleState(viewModelToPersist, selectedRole));
        return RedirectToAction(nameof(ChangeInviteRoleCheck), new { organisationSlug, inviteGuid });
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change/check")]
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

    [HttpPost("invites/{inviteGuid:guid}/organisation-role/change/check")]
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

        // Idempotency: if the role has already been applied to the invite, treat as success
        var currentViewModel =
            await organisationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (currentViewModel is not null && currentViewModel.CurrentRole == state.SelectedRole)
        {
            return RedirectToAction(nameof(ChangeInviteRoleSuccess), new { organisationSlug, inviteGuid });
        }

        var success = await organisationRoleFlowService.UpdateInviteRoleAsync(
            organisationSlug,
            inviteGuid,
            state.SelectedRole,
            ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeInviteRoleSuccess), new { organisationSlug, inviteGuid })!);
    }

    [HttpGet("invites/{inviteGuid:guid}/organisation-role/change/success")]
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

        var roleDescription = (await organisationRoleService.GetRoleAsync(state.SelectedRole, ct))?.Description ??
                              string.Empty;
        return View("ChangeRoleSuccess", new ChangeUserRoleSuccessViewModel(
            OrganisationSlug: organisationSlug,
            UserDisplayName: state.UserDisplayName,
            NewRole: state.SelectedRole,
            RoleDescription: roleDescription));
    }

    [HttpGet("user/{cdpPersonId:guid}/application-roles/change")]
    public async Task<IActionResult> ChangeApplicationRoles(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var viewModel =
            await applicationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, cdpPersonId, null);
        if (state is not null)
        {
            var stateByOrgAppId = state.Applications.ToDictionary(a => a.OrganisationApplicationId);
            foreach (var app in viewModel.Applications)
            {
                if (stateByOrgAppId.TryGetValue(app.OrganisationApplicationId, out var stateApp))
                {
                    app.GiveAccess = stateApp.GiveAccess;
                    app.SelectedRoleId = stateApp.SelectedRoleId;
                }
            }
        }

        return View("ChangeApplicationRoles", viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/application-roles/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeApplicationRolesSubmit(
        string organisationSlug,
        Guid cdpPersonId,
        ApplicationRoleChangePostModel input,
        CancellationToken ct)
    {
        var viewModel =
            await applicationRoleFlowService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        return await HandleApplicationRolesSubmit(
            organisationSlug, cdpPersonId, null, input, viewModel,
            nameof(ChangeApplicationRolesCheck), ct);
    }

    [HttpGet("user/{cdpPersonId:guid}/application-roles/change/check")]
    public async Task<IActionResult> ChangeApplicationRolesCheck(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, cdpPersonId, null);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });
        }

        return View("CheckApplicationRoles", BuildCheckViewModel(state));
    }

    [HttpPost("user/{cdpPersonId:guid}/application-roles/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeApplicationRolesCheckSubmit(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, cdpPersonId, null);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });
        }

        var assignments = BuildAssignmentPostModels(state);
        var success =
            await applicationRoleFlowService.UpdateUserRolesAsync(organisationSlug, cdpPersonId, assignments, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeApplicationRolesSuccess), new { organisationSlug, cdpPersonId })!);
    }

    [HttpGet("user/{cdpPersonId:guid}/application-roles/change/success")]
    public async Task<IActionResult> ChangeApplicationRolesSuccess(
        string organisationSlug,
        Guid cdpPersonId,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, cdpPersonId, null);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });
        }

        await changeApplicationRoleStateStore.ClearAsync();

        var changedApplications = state.Applications
            .Where(a => !a.HasExistingAccess && a.GiveAccess || a.HasExistingAccess && HasRoleChanged(a))
            .Select(a => new ChangedApplicationRoleViewModel
            {
                ApplicationName = a.ApplicationName,
                CurrentRoleName = a.CurrentRoleName,
                NewRoleName = a.SelectedRoleName,
                IsNewAssignment = !a.HasExistingAccess
            })
            .ToList();

        if (changedApplications.Count == 0)
        {
            return RedirectToAction(nameof(ChangeApplicationRoles), new { organisationSlug, cdpPersonId });
        }

        return View("ChangeApplicationRolesSuccess", new ChangeApplicationRolesSuccessViewModel
        {
            OrganisationSlug = organisationSlug,
            UserDisplayName = state.UserDisplayName,
            ChangedApplications = changedApplications
        });
    }

    [HttpGet("user/{cdpPersonId:guid}/application/{clientId}/remove/success")]
    public async Task<IActionResult> RemoveApplicationSuccess(
        string organisationSlug,
        Guid cdpPersonId,
        string clientId,
        CancellationToken ct)
    {
        var viewModel = await userDetailsQueryService.GetRemoveApplicationSuccessViewModelAsync(
            organisationSlug,
            cdpPersonId,
            clientId,
            ct);

        if (viewModel is null)
        {
            return RedirectToAction(nameof(Index), new { organisationSlug });
        }

        return View("RemoveApplicationSuccess", viewModel);
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change")]
    public async Task<IActionResult> ChangeInviteApplicationRoles(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var viewModel =
            await applicationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, null, inviteGuid);
        if (state is not null)
        {
            var stateByOrgAppId = state.Applications.ToDictionary(a => a.OrganisationApplicationId);
            foreach (var app in viewModel.Applications)
            {
                if (stateByOrgAppId.TryGetValue(app.OrganisationApplicationId, out var stateApp))
                {
                    app.GiveAccess = stateApp.GiveAccess;
                    app.SelectedRoleId = stateApp.SelectedRoleId;
                }
            }
        }

        return View("ChangeApplicationRoles", viewModel);
    }

    [HttpPost("invites/{inviteGuid:guid}/application-roles/change")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteApplicationRolesSubmit(
        string organisationSlug,
        Guid inviteGuid,
        ApplicationRoleChangePostModel input,
        CancellationToken ct)
    {
        var viewModel =
            await applicationRoleFlowService.GetInviteViewModelAsync(organisationSlug, inviteGuid, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        return await HandleApplicationRolesSubmit(
            organisationSlug, null, inviteGuid, input, viewModel,
            nameof(ChangeInviteApplicationRolesCheck), ct);
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change/check")]
    public async Task<IActionResult> ChangeInviteApplicationRolesCheck(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, null, inviteGuid);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });
        }

        return View("CheckApplicationRoles", BuildCheckViewModel(state));
    }

    [HttpPost("invites/{inviteGuid:guid}/application-roles/change/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteApplicationRolesCheckSubmit(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, null, inviteGuid);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });
        }

        var assignments = BuildAssignmentPostModels(state);
        var success =
            await applicationRoleFlowService.UpdateInviteRolesAsync(organisationSlug, inviteGuid, assignments, ct);
        return success.Match<IActionResult>(
            _ => Redirect("/error"),
            outcome => outcome == ServiceOutcome.NotFound
                ? NotFound()
                : RedirectToAction(nameof(ChangeInviteApplicationRolesSuccess), new { organisationSlug, inviteGuid })!);
    }

    [HttpGet("invites/{inviteGuid:guid}/application-roles/change/success")]
    public async Task<IActionResult> ChangeInviteApplicationRolesSuccess(
        string organisationSlug,
        Guid inviteGuid,
        CancellationToken ct)
    {
        var state = await GetValidatedChangeApplicationRoleStateAsync(organisationSlug, null, inviteGuid);
        if (state is null)
        {
            return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });
        }

        await changeApplicationRoleStateStore.ClearAsync();

        var changedApplications = state.Applications
            .Where(a => !a.HasExistingAccess && a.GiveAccess || a.HasExistingAccess && HasRoleChanged(a))
            .Select(a => new ChangedApplicationRoleViewModel
            {
                ApplicationName = a.ApplicationName,
                CurrentRoleName = a.CurrentRoleName,
                NewRoleName = a.SelectedRoleName,
                IsNewAssignment = !a.HasExistingAccess
            })
            .ToList();

        if (changedApplications.Count == 0)
        {
            return RedirectToAction(nameof(ChangeInviteApplicationRoles), new { organisationSlug, inviteGuid });
        }

        return View("ChangeApplicationRolesSuccess", new ChangeApplicationRolesSuccessViewModel
        {
            OrganisationSlug = organisationSlug,
            UserDisplayName = state.UserDisplayName,
            ChangedApplications = changedApplications
        });
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

    private static ChangeRoleState
        ToChangeRoleState(ChangeUserRoleViewModel viewModel, OrganisationRole selectedRole) =>
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

    private async Task<OrganisationRoleStepViewModel> BuildOrganisationRoleStepViewModelAsync(
        InviteUserState state,
        bool returnToCheckAnswers,
        CancellationToken ct)
    {
        return new OrganisationRoleStepViewModel(
            state.OrganisationSlug,
            state.FirstName,
            state.LastName,
            state.Email,
            state.OrganisationRole,
            returnToCheckAnswers,
            (await organisationRoleService.GetRolesAsync(ct)).ToOptions());
    }

    private async Task<ChangeUserRolePageViewModel> BuildChangeUserRolePageViewModelAsync(
        ChangeUserRoleViewModel viewModel,
        OrganisationRole? selectedRole,
        CancellationToken ct)
    {
        return ChangeUserRolePageViewModel.From(
            viewModel,
            (await organisationRoleService.GetRolesAsync(ct)).ToOptions(),
            selectedRole);
    }

    private async Task<IActionResult> HandleApplicationRolesSubmit(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid,
        ApplicationRoleChangePostModel input,
        ChangeUserApplicationRolesViewModel viewModel,
        string checkActionName,
        CancellationToken ct)
    {
        // Capture current (API) role IDs before applying the posted values
        var originalRoles =
            viewModel.Applications.ToDictionary(a => a.OrganisationApplicationId, a => a.SelectedRoleId);
        var originalRoleIds =
            viewModel.Applications.ToDictionary(a => a.OrganisationApplicationId, a => a.SelectedRoleIds.ToList());

        // Apply posted selections to view model so errors re-display correct state
        var postedMap = input.Applications.ToDictionary(a => a.OrganisationApplicationId, a => a);
        foreach (var app in viewModel.Applications)
        {
            if (postedMap.TryGetValue(app.OrganisationApplicationId, out var posted))
            {
                app.GiveAccess = app.HasExistingAccess || posted.GiveAccess;
                if (app.AllowsMultipleRoleAssignments)
                {
                    app.SelectedRoleIds = posted.SelectedRoleIds;
                    app.SelectedRoleId = posted.SelectedRoleIds.Count > 0 ? posted.SelectedRoleIds[0] : null;
                }
                else
                {
                    app.SelectedRoleId = posted.SelectedRoleId;
                }
            }
        }

        // Validate: for any newly-granted app, a role must be selected
        bool hasRoleError = false;
        foreach (var item in viewModel.Applications.Select((app, i) => (app, i)))
        {
            if (item.app.GiveAccess && !HasRoleSelected(item.app))
            {
                ModelState.AddModelError($"Applications[{item.i}].SelectedRoleId",
                    "Select a role for this application");
                hasRoleError = true;
            }
        }

        // Validate: something must have actually changed (new access grant or different role)
        var anyNewAccess = viewModel.Applications.Any(app =>
            !app.HasExistingAccess && app.GiveAccess);

        var anyRoleChanged = viewModel.Applications.Any(app =>
            app.HasExistingAccess && RolesChanged(app, originalRoles, originalRoleIds));

        if (!hasRoleError && !anyNewAccess && !anyRoleChanged)
        {
            ModelState.AddModelError("Applications",
                "Select a different role or grant access to at least one application to continue");
            return View("ChangeApplicationRoles", viewModel);
        }

        if (hasRoleError)
        {
            return View("ChangeApplicationRoles", viewModel);
        }

        var assignmentStates = viewModel.Applications
            .Where(app => app.HasExistingAccess || app.GiveAccess)
            .Select(app =>
            {
                var origRoleId = originalRoles.TryGetValue(app.OrganisationApplicationId, out var orig) ? orig : null;
                var origRoleIds = originalRoleIds.TryGetValue(app.OrganisationApplicationId, out var origIds)
                    ? origIds
                    : new List<int>();
                var newRoleIds = app.AllowsMultipleRoleAssignments
                    ? app.SelectedRoleIds
                    : (app.SelectedRoleId.HasValue ? new List<int> { app.SelectedRoleId.Value } : new List<int>());
                var origRoleNames = origRoleIds.Count > 0
                    ? string.Join(", ",
                        origRoleIds.Select(id => app.Roles.FirstOrDefault(r => r.Id == id)?.Name ?? string.Empty)
                            .Where(n => n != string.Empty))
                    : (origRoleId.HasValue
                        ? app.Roles.FirstOrDefault(r => r.Id == origRoleId)?.Name ?? string.Empty
                        : string.Empty);
                var newRoleName = string.Join(", ",
                    newRoleIds.Select(id => app.Roles.FirstOrDefault(r => r.Id == id)?.Name ?? string.Empty)
                        .Where(n => n != string.Empty));
                return new ApplicationRoleAssignmentState(
                    app.OrganisationApplicationId,
                    app.ApplicationId,
                    app.ApplicationName,
                    app.HasExistingAccess,
                    app.GiveAccess,
                    origRoleIds.Count > 0 ? origRoleIds[0] : origRoleId,
                    origRoleNames,
                    newRoleIds.Count > 0 ? newRoleIds[0] : null,
                    newRoleName,
                    SelectedRoleIds: newRoleIds,
                    CurrentRoleIds: origRoleIds.Count > 0 ? origRoleIds : null);
            }).ToList();

        var state = new ChangeApplicationRoleState(
            organisationSlug,
            cdpPersonId,
            inviteGuid,
            viewModel.UserDisplayName,
            viewModel.Email,
            assignmentStates);

        await changeApplicationRoleStateStore.SetAsync(state);

        object routeValues = cdpPersonId.HasValue
            ? new { organisationSlug, cdpPersonId }
            : new { organisationSlug, inviteGuid };

        return RedirectToAction(checkActionName, routeValues);
    }

    private static ChangeApplicationRolesCheckViewModel BuildCheckViewModel(ChangeApplicationRoleState state) =>
        new()
        {
            OrganisationSlug = state.OrganisationSlug,
            UserDisplayName = state.UserDisplayName,
            Email = state.Email,
            IsPending = state.InviteGuid.HasValue,
            CdpPersonId = state.CdpPersonId,
            InviteGuid = state.InviteGuid,
            ChangedApplications = state.Applications
                .Where(a => !a.HasExistingAccess && a.GiveAccess || a.HasExistingAccess && HasRoleChanged(a))
                .Select(a => new ChangedApplicationRoleViewModel
                {
                    ApplicationName = a.ApplicationName,
                    CurrentRoleName = a.CurrentRoleName,
                    NewRoleName = a.SelectedRoleName,
                    IsNewAssignment = !a.HasExistingAccess
                })
                .ToList()
        };

    private static IReadOnlyList<ApplicationRoleAssignmentPostModel> BuildAssignmentPostModels(
        ChangeApplicationRoleState state) =>
        state.Applications
            .Where(a => a.GiveAccess && (a.SelectedRoleIds is { Count: > 0 } || a.SelectedRoleId.HasValue))
            .Select(a => new ApplicationRoleAssignmentPostModel
            {
                OrganisationApplicationId = a.OrganisationApplicationId,
                ApplicationId = a.ApplicationId,
                GiveAccess = a.GiveAccess,
                SelectedRoleId = a.SelectedRoleId,
                SelectedRoleIds = a.SelectedRoleIds?.ToList() ?? []
            })
            .ToList();

    private async Task<ChangeApplicationRoleState?> GetValidatedChangeApplicationRoleStateAsync(
        string organisationSlug,
        Guid? cdpPersonId,
        Guid? inviteGuid)
    {
        var state = await changeApplicationRoleStateStore.GetAsync();
        if (state is null)
        {
            return null;
        }

        if (!state.OrganisationSlug.Equals(organisationSlug, StringComparison.OrdinalIgnoreCase) ||
            state.CdpPersonId != cdpPersonId ||
            state.InviteGuid != inviteGuid)
        {
            await changeApplicationRoleStateStore.ClearAsync();
            return null;
        }

        return state;
    }

    private static bool HasRoleSelected(ApplicationAccessSelectionViewModel app) =>
        app.AllowsMultipleRoleAssignments
            ? app.SelectedRoleIds.Count > 0
            : app.SelectedRoleId.HasValue && app.Roles.Any(r => r.Id == app.SelectedRoleId.Value);

    private static bool HasRoleSelected(ApplicationRoleChangeViewModel app) =>
        app.AllowsMultipleRoleAssignments
            ? app.SelectedRoleIds.Count > 0
            : app.SelectedRoleId.HasValue;

    private static bool HasRoleChanged(ApplicationRoleAssignmentState a)
    {
        var selected =
            (a.SelectedRoleIds ?? (a.SelectedRoleId.HasValue ? [a.SelectedRoleId.Value] : [])).OrderBy(x => x);
        var current = (a.CurrentRoleIds ?? (a.CurrentRoleId.HasValue ? [a.CurrentRoleId.Value] : new List<int>()))
            .OrderBy(x => x);
        return !selected.SequenceEqual(current);
    }

    private static bool RolesChanged(
        ApplicationRoleChangeViewModel app,
        Dictionary<int, int?> originalSingleRoles,
        Dictionary<int, List<int>> originalMultiRoles)
    {
        if (app.AllowsMultipleRoleAssignments)
        {
            var orig = originalMultiRoles.TryGetValue(app.OrganisationApplicationId, out var ids)
                ? ids
                : new List<int>();
            return !orig.OrderBy(x => x).SequenceEqual(app.SelectedRoleIds.OrderBy(x => x));
        }

        return originalSingleRoles.TryGetValue(app.OrganisationApplicationId, out var origId) &&
               origId != app.SelectedRoleId;
    }

    private bool IsSelf(string? userEmail)
    {
        var currentUserEmail = User.FindFirst("email")?.Value;
        return !string.IsNullOrEmpty(userEmail) &&
               !string.IsNullOrEmpty(currentUserEmail) &&
               string.Equals(currentUserEmail, userEmail, StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet("user/{cdpPersonId:guid}/remove")]
    public async Task<IActionResult> RemoveUser(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        if (await userRemovalService.IsLastOwnerAsync(organisationSlug, cdpPersonId, ct))
        {
            ModelState.AddModelError(string.Empty, "You cannot remove the last owner of the organisation.");
            return View("Remove", viewModel);
        }

        if (IsSelf(viewModel.Email))
        {
            ModelState.AddModelError(string.Empty, "You cannot remove yourself from the organisation.");
        }

        return View("Remove", viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(
        string organisationSlug,
        Guid cdpPersonId,
        RemoveUserViewModel input,
        CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetUserViewModelAsync(organisationSlug, cdpPersonId, ct);
        if (viewModel is null)
        {
            return NotFound();
        }

        if (IsSelf(viewModel.Email))
        {
            ModelState.AddModelError(string.Empty, "You cannot remove yourself from the organisation.");
            return View("Remove", viewModel);
        }

        // Permission check: prevent Admins from removing Owners
        if (await IsAdminTargetingOwnerAsync(organisationSlug, viewModel.CurrentRole, ct))
        {
            ModelState.AddModelError(string.Empty, "You do not have permission to remove an Owner.");
            return View("Remove", viewModel);
        }


        if (input.RemoveConfirmed == false)
        {
            return RedirectToAction(nameof(Index), new { organisationSlug });
        }

        if (!ModelState.IsValid)
        {
            return View("Remove", viewModel);
        }

        // Re-check last-owner status immediately before performing the removal to avoid race conditions
        if (cdpPersonId != Guid.Empty)
        {
            var isLastOwnerNow = await userRemovalService.IsLastOwnerAsync(organisationSlug, cdpPersonId, ct);
            if (isLastOwnerNow)
            {
                ModelState.AddModelError(string.Empty, "You cannot remove the last owner of the organisation.");
                return View("Remove", viewModel);
            }
        }

        var result = await userRemovalService.RemoveUserAsync(organisationSlug, cdpPersonId, ct);
        var success = result.Match(_ => false, outcome => outcome == ServiceOutcome.Success);
        return success ? RedirectToAction(nameof(RemoveSuccess), new { organisationSlug }) : NotFound();
    }

    [HttpGet("invites/{pendingInviteId:int}/remove")]
    public async Task<IActionResult> RemoveInvite(string organisationSlug, int pendingInviteId, CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetInviteViewModelAsync(organisationSlug, pendingInviteId, ct);
        return viewModel is null ? NotFound() : View("Remove", viewModel);
    }

    [HttpPost("invites/{pendingInviteId:int}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveInvite(
        string organisationSlug,
        int pendingInviteId,
        RemoveUserViewModel input,
        CancellationToken ct)
    {
        if (input.RemoveConfirmed == false)
        {
            return RedirectToAction(nameof(Index), new { organisationSlug });
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userRemovalService.GetInviteViewModelAsync(organisationSlug, pendingInviteId, ct);
            return viewModel is null ? NotFound() : View("Remove", viewModel);
        }

        var result = await userRemovalService.RemoveInviteAsync(organisationSlug, pendingInviteId, ct);
        var success = result.Match(_ => false, outcome => outcome == ServiceOutcome.Success);
        return success ? RedirectToAction(nameof(RemoveSuccess), new { organisationSlug }) : NotFound();
    }

    [HttpGet("remove/success")]
    public IActionResult RemoveSuccess(string organisationSlug)
    {
        return View("RemoveSuccess");
    }

    [HttpGet("user/{cdpPersonId:guid}")]
    public async Task<IActionResult> Details(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userDetailsQueryService.GetViewModelAsync(organisationSlug, cdpPersonId, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpGet("user/{cdpPersonId:guid}/application/{clientId}/remove/check")]
    public async Task<IActionResult> RemoveApplication(
        string organisationSlug,
        Guid cdpPersonId,
        string clientId,
        CancellationToken ct)
    {
        var viewModel = await userRemovalService.GetRemoveApplicationViewModelAsync(organisationSlug, cdpPersonId, clientId, ct);
        return viewModel is null ? NotFound() : View("RemoveApplicationCheck", viewModel);
    }

    [HttpPost("user/{cdpPersonId:guid}/application/{clientId}/remove/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveApplication(
        string organisationSlug,
        Guid cdpPersonId,
        string clientId,
        RemoveApplicationViewModel input,
        CancellationToken ct)
    {
        if (input.RevokeConfirmed == false)
        {
            return RedirectToAction(nameof(Details), new { organisationSlug, cdpPersonId });
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userRemovalService.GetRemoveApplicationViewModelAsync(organisationSlug, cdpPersonId, clientId, ct);
            return viewModel is null ? NotFound() : View("RemoveApplicationCheck", viewModel);
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

    // Helper: Prevent Admins from changing/removing Owners
    private async Task<bool> IsAdminTargetingOwnerAsync(
        string organisationSlug, OrganisationRole targetRole, CancellationToken ct)
    {
        if (targetRole != OrganisationRole.Owner) return false;

        var cdpClaimsJson = User.FindFirst("cdp_claims")?.Value;
        var userClaims = JsonHelper.TryDeserialize<UserClaims>(cdpClaimsJson);
        if (userClaims is null) return false;

        var orgId = await adapter.ResolveOrganisationIdAsync(organisationSlug, ct);
        if (orgId is null) return false;

        var currentRole = userClaims.Organisations
            .FirstOrDefault(o => o.OrganisationId == orgId.Value)
            ?.OrganisationRole;

        return string.Equals(currentRole, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}