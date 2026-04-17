using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Application.JoinRequests;
using CO.CDP.UserManagement.App.Controllers;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.Controllers;

public class JoinRequestsControllerTests
{
    private readonly Mock<IJoinRequestFlowService> _flowService = new();
    private readonly Guid _joinRequestId = Guid.NewGuid();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();

    private JoinRequestsController CreateSut() => new(_flowService.Object);

    private JoinRequestConfirmViewModel MakeViewModel(JoinRequestAction action) =>
        new(_organisationId, _joinRequestId, _personId, "Alice Brown", "alice@example.com", action);

    // ── Approve GET ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_WhenViewModelFound_ReturnsConfirmView()
    {
        _flowService
            .Setup(s => s.GetConfirmViewModelAsync(
                _organisationId, _joinRequestId, _personId, JoinRequestAction.Approve, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeViewModel(JoinRequestAction.Approve));

        var result = await CreateSut().Approve(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("JoinRequestConfirm");
        var vm = view.Model.Should().BeOfType<JoinRequestConfirmViewModel>().Subject;
        vm.Action.Should().Be(JoinRequestAction.Approve);
        vm.FullName.Should().Be("Alice Brown");
    }

    [Fact]
    public async Task Approve_WhenViewModelNull_ReturnsNotFound()
    {
        _flowService
            .Setup(s => s.GetConfirmViewModelAsync(
                _organisationId, _joinRequestId, _personId, JoinRequestAction.Approve, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JoinRequestConfirmViewModel?)null);

        var result = await CreateSut().Approve(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── ApproveConfirm POST ────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveConfirm_WhenSuccess_RedirectsToUsersList()
    {
        _flowService
            .Setup(s => s.ApproveAsync(_organisationId, _joinRequestId, _personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result =
            await CreateSut().ApproveConfirm(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersListController.Index));
        redirect.ControllerName.Should().Be("UsersList");
    }

    [Fact]
    public async Task ApproveConfirm_WhenNotFound_ReturnsNotFound()
    {
        _flowService
            .Setup(s => s.ApproveAsync(_organisationId, _joinRequestId, _personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound));

        var result =
            await CreateSut().ApproveConfirm(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ApproveConfirm_WhenFailure_RedirectsToError()
    {
        _flowService
            .Setup(s => s.ApproveAsync(_organisationId, _joinRequestId, _personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Failure(ServiceFailure.Unexpected));

        var result =
            await CreateSut().ApproveConfirm(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/error");
    }

    // ── Reject GET ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reject_WhenViewModelFound_ReturnsConfirmView()
    {
        _flowService
            .Setup(s => s.GetConfirmViewModelAsync(
                _organisationId, _joinRequestId, _personId, JoinRequestAction.Reject, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeViewModel(JoinRequestAction.Reject));

        var result = await CreateSut().Reject(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("JoinRequestConfirm");
        var vm = view.Model.Should().BeOfType<JoinRequestConfirmViewModel>().Subject;
        vm.Action.Should().Be(JoinRequestAction.Reject);
    }

    [Fact]
    public async Task Reject_WhenViewModelNull_ReturnsNotFound()
    {
        _flowService
            .Setup(s => s.GetConfirmViewModelAsync(
                _organisationId, _joinRequestId, _personId, JoinRequestAction.Reject, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JoinRequestConfirmViewModel?)null);

        var result = await CreateSut().Reject(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── RejectConfirm POST ─────────────────────────────────────────────────────

    [Fact]
    public async Task RejectConfirm_WhenSuccess_RedirectsToUsersList()
    {
        _flowService
            .Setup(s => s.RejectAsync(_organisationId, _joinRequestId, _personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success));

        var result =
            await CreateSut().RejectConfirm(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsersListController.Index));
        redirect.ControllerName.Should().Be("UsersList");
    }

    [Fact]
    public async Task RejectConfirm_WhenFailure_RedirectsToError()
    {
        _flowService
            .Setup(s => s.RejectAsync(_organisationId, _joinRequestId, _personId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServiceFailure, ServiceOutcome>.Failure(ServiceFailure.Unexpected));

        var result =
            await CreateSut().RejectConfirm(_organisationId, _joinRequestId, _personId, CancellationToken.None);

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/error");
    }
}