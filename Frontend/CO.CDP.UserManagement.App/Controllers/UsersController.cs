using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize]
[Route("organisation/{organisationSlug}/users")]
public class UsersController(IUserService userService) : Controller
{
    [HttpGet]
    [Route("")]
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

    [HttpGet("add")]
    public async Task<IActionResult> Add(string organisationSlug, CancellationToken ct)
    {
        var viewModel = await userService.GetInviteUserViewModelAsync(organisationSlug, ct: ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string organisationSlug, InviteUserViewModel input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var viewModel = await userService.GetInviteUserViewModelAsync(organisationSlug, input, ct);
            return viewModel is null ? NotFound() : View(viewModel);
        }

        var success = await userService.InviteUserAsync(organisationSlug, input, ct);
        return success ? RedirectToAction(nameof(Index), new { organisationSlug }) : NotFound();
    }

    [HttpGet("invites/{pendingInviteId:int}/resend-invite")]
    public async Task<IActionResult> ResendInvite(string organisationSlug, int pendingInviteId, CancellationToken ct)
    {
        var success = await userService.ResendInviteAsync(organisationSlug, pendingInviteId, ct);
        return success ? RedirectToAction(nameof(Index), new { organisationSlug }) : NotFound();
    }

    [HttpGet("{cdpPersonId:guid}/change-role")]
    public async Task<IActionResult> ChangeRole(string organisationSlug, Guid cdpPersonId, CancellationToken ct)
    {
        var viewModel = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, cdpPersonId, null, ct);
        return viewModel is null ? NotFound() : View(viewModel);
    }

    [HttpPost("{cdpPersonId:guid}/change-role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(
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
            return viewModel is null ? NotFound() : View(viewModel);
        }

        var resolvedRole = organisationRole ?? OrganisationRole.Member;
        var success = await userService.UpdateUserRoleAsync(
            organisationSlug,
            cdpPersonId,
            null,
            resolvedRole,
            ct);
        return success ? RedirectToAction(nameof(Index), new { organisationSlug }) : NotFound();
    }

    [HttpGet("invites/{pendingInviteId:int}/change-role")]
    public async Task<IActionResult> ChangeInviteRole(string organisationSlug, int pendingInviteId, CancellationToken ct)
    {
        var viewModel = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, null, pendingInviteId, ct);
        return viewModel is null ? NotFound() : View("ChangeRole", viewModel);
    }

    [HttpPost("invites/{pendingInviteId:int}/change-role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeInviteRole(
        string organisationSlug,
        int pendingInviteId,
        OrganisationRole? organisationRole,
        CancellationToken ct)
    {
        if (organisationRole == null)
        {
            ModelState.AddModelError(nameof(organisationRole), "Select an organisation role");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await userService.GetChangeUserRoleViewModelAsync(organisationSlug, null, pendingInviteId, ct);
            return viewModel is null ? NotFound() : View("ChangeRole", viewModel);
        }

        var resolvedRole = organisationRole ?? OrganisationRole.Member;
        var success = await userService.UpdateUserRoleAsync(
            organisationSlug,
            null,
            pendingInviteId,
            resolvedRole,
            ct);
        return success ? RedirectToAction(nameof(Index), new { organisationSlug }) : NotFound();
    }
}
