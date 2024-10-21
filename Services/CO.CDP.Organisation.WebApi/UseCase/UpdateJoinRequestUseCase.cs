using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateJoinRequestUseCase(
    IOrganisationRepository organisationRepository,
    IOrganisationJoinRequestRepository requestRepository,
    IPersonRepository personRepository)
    : IUseCase<(Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid joinRequestId, UpdateJoinRequest updateJoinRequest) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var joinRequest = await requestRepository.Find(command.joinRequestId, command.organisationId)
            ?? throw new UnknownOrganisationJoinRequestException($"Unknown organisation join request for org id {command.organisationId} or request id {command.joinRequestId}.");

        var person = await personRepository.Find(command.updateJoinRequest.ReviewedBy)
            ?? throw new UnknownPersonException($"Unknown person {command.updateJoinRequest.ReviewedBy}.");

        joinRequest.ReviewedById = person.Id;
        joinRequest.Status = command.updateJoinRequest.status;
        joinRequest.ReviewedOn = DateTimeOffset.UtcNow;

        if (command.updateJoinRequest.status == OrganisationInformation.OrganisationJoinRequestStatus.Accepted)
        {
            organisation.OrganisationPersons.Add(new OrganisationPerson
            {
                Person = joinRequest.Person!,
                Organisation = organisation,
                Scopes = []
            });

            organisationRepository.Save(organisation);
        }
        
        requestRepository.Save(joinRequest);

        return await Task.FromResult(true);
    }
}