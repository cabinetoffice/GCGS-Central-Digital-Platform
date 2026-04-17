using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Application.JoinRequests;

public interface IJoinRequestFlowService
{
    Task<JoinRequestConfirmViewModel?> GetConfirmViewModelAsync(
        Guid organisationId,
        Guid joinRequestId,
        Guid personId,
        JoinRequestAction action,
        CancellationToken ct = default);

    Task<Result<ServiceFailure, ServiceOutcome>> ApproveAsync(
        Guid organisationId,
        Guid joinRequestId,
        Guid personId,
        CancellationToken ct = default);

    Task<Result<ServiceFailure, ServiceOutcome>> RejectAsync(
        Guid organisationId,
        Guid joinRequestId,
        Guid personId,
        CancellationToken ct = default);
}