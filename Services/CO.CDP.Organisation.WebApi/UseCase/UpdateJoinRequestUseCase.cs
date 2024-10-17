using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateJoinRequestUseCase(
    IOrganisationRepository organisationRepository,
    IOrganisationJoinRequestRepository requestRepository)
    : IUseCase<(Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest) command)
    {
        _ = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var joinRequest = await requestRepository.Find(command.joinRequestId, command.organisationId)
            ?? throw new UnknownOrganisationJoinRequestException ($"Unknown organisation join request for org id {command.organisationId} or request id {command.joinRequestId}.");

        joinRequest.ReviewedById = command.updateJoinRequest.ReviewedBy;
        joinRequest.Status = command.updateJoinRequest.status;
        joinRequest.ReviewedOn = DateTimeOffset.UtcNow;

        requestRepository.Save(joinRequest);

        return await Task.FromResult(true);
    }
}