using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;

namespace CO.CDP.UserManagement.App.Application.JoinRequests.Implementations;

public class JoinRequestFlowService(IUserManagementApiAdapter adapter) : IJoinRequestFlowService
{
    public async Task<JoinRequestConfirmViewModel?> GetConfirmViewModelAsync(
        Guid organisationId,
        Guid joinRequestId,
        Guid personId,
        JoinRequestAction action,
        CancellationToken ct = default)
    {
        var org = await adapter.GetOrganisationByGuidAsync(organisationId, ct);
        if (org is null) return null;

        var joinRequests = await adapter.GetJoinRequestsAsync(org.CdpOrganisationGuid, ct);
        var request = joinRequests.FirstOrDefault(r => r.Id == joinRequestId && r.PersonId == personId);
        if (request is null) return null;

        return new JoinRequestConfirmViewModel(
            organisationId,
            joinRequestId,
            personId,
            $"{request.FirstName} {request.LastName}",
            request.Email,
            action);
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> ApproveAsync(
        Guid organisationId,
        Guid joinRequestId,
        Guid personId,
        CancellationToken ct = default)
    {
        var org = await adapter.GetOrganisationByGuidAsync(organisationId, ct);
        if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        var joinRequests = await adapter.GetJoinRequestsAsync(org.CdpOrganisationGuid, ct);
        var request = joinRequests.FirstOrDefault(r => r.Id == joinRequestId && r.PersonId == personId);
        if (request is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        return await adapter.ReviewJoinRequestAsync(
            org.CdpOrganisationGuid,
            joinRequestId,
            new ReviewJoinRequestRequest { Decision = JoinRequestDecision.Accepted, RequestingPersonId = personId },
            ct);
    }

    public async Task<Result<ServiceFailure, ServiceOutcome>> RejectAsync(
        Guid organisationId,
        Guid joinRequestId,
        Guid personId,
        CancellationToken ct = default)
    {
        var org = await adapter.GetOrganisationByGuidAsync(organisationId, ct);
        if (org is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        var joinRequests = await adapter.GetJoinRequestsAsync(org.CdpOrganisationGuid, ct);
        var request = joinRequests.FirstOrDefault(r => r.Id == joinRequestId && r.PersonId == personId);
        if (request is null) return Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

        return await adapter.ReviewJoinRequestAsync(
            org.CdpOrganisationGuid,
            joinRequestId,
            new ReviewJoinRequestRequest { Decision = JoinRequestDecision.Rejected, RequestingPersonId = personId },
            ct);
    }
}