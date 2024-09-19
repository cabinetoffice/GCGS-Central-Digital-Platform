using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class ReviewOrganisationUseCase(IOrganisationRepository organisationRepository, IPersonRepository personRepository)
    : IUseCase<ReviewOrganisation, Boolean>
{
    // This is subject to change based on feedback from Cabinet Office
    public async Task<Boolean> Execute(ReviewOrganisation command)
    {
        var organisation = await organisationRepository.Find(command.OrganisationId);
        var person = await personRepository.Find(command.approvedById);

        if (command.Approved)
        {
            organisation.ApprovedOn = new DateTimeOffset();
            organisation.ApprovedBy = person;
        }

        organisation.ApprovedComment = command.Comment;

        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}