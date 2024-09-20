using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class ReviewOrganisationUseCase(IOrganisationRepository organisationRepository, IPersonRepository personRepository)
    : IUseCase<ReviewOrganisation, Boolean>
{
    public async Task<Boolean> Execute(ReviewOrganisation command)
    {
        var organisation = await organisationRepository.Find(command.OrganisationId) ?? throw new UnknownOrganisationException($"Unknown organisation {command.OrganisationId}.");
        var person = await personRepository.Find(command.approvedById) ?? throw new UnknownPersonException($"Unknown person {command.approvedById}.");

        // Organisation is left in a pending state until user approves it.
        if (command.Approved)
        {
            organisation.ApprovedOn = DateTimeOffset.UtcNow;
        }

        organisation.ApprovedBy = person;
        organisation.ApprovedComment = command.Comment;
        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}