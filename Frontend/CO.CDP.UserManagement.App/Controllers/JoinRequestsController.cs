using CO.CDP.UserManagement.App.Application.JoinRequests;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

public class JoinRequestsController(IJoinRequestFlowService joinRequestFlowService) : UsersBaseController
{
    [HttpGet("join-requests/{joinRequestId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id, Guid joinRequestId, [FromQuery] Guid personId, CancellationToken ct)
    {
        var vm = await joinRequestFlowService.GetConfirmViewModelAsync(
            id, joinRequestId, personId, JoinRequestAction.Approve, ct);
        return vm is null ? NotFound() : View("JoinRequestConfirm", vm);
    }

    [HttpPost("join-requests/{joinRequestId:guid}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveConfirm(
        Guid id, Guid joinRequestId, [FromQuery] Guid personId, CancellationToken ct)
    {
        var result = await joinRequestFlowService.ApproveAsync(id, joinRequestId, personId, ct);
        if (result.IsFailure) return Redirect("/error");
        if (result.Match(_ => false, outcome => outcome == ServiceOutcome.NotFound)) return NotFound();
        return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
    }

    [HttpGet("join-requests/{joinRequestId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id, Guid joinRequestId, [FromQuery] Guid personId, CancellationToken ct)
    {
        var vm = await joinRequestFlowService.GetConfirmViewModelAsync(
            id, joinRequestId, personId, JoinRequestAction.Reject, ct);
        return vm is null ? NotFound() : View("JoinRequestConfirm", vm);
    }

    [HttpPost("join-requests/{joinRequestId:guid}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectConfirm(
        Guid id, Guid joinRequestId, [FromQuery] Guid personId, CancellationToken ct)
    {
        var result = await joinRequestFlowService.RejectAsync(id, joinRequestId, personId, ct);
        if (result.IsFailure) return Redirect("/error");
        if (result.Match(_ => false, outcome => outcome == ServiceOutcome.NotFound)) return NotFound();
        return RedirectToAction(nameof(UsersListController.Index), "UsersList", new { id });
    }
}