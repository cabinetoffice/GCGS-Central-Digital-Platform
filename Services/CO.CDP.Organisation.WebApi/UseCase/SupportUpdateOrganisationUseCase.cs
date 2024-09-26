using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;
public class SupportUpdateOrganisationUseCase(IOrganisationRepository organisationRepository, IPersonRepository personRepository)
    : IUseCase<(Guid organisationId, SupportUpdateOrganisation supportUpdateOrganisation), bool>
{
    public async Task<bool> Execute((Guid organisationId, SupportUpdateOrganisation supportUpdateOrganisation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId) ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        switch (command.supportUpdateOrganisation.Type)
        {
            case SupportOrganisationUpdateType.Review:

                var personId = command.supportUpdateOrganisation.Organisation.ApprovedById;

                var person = await personRepository.Find(personId) ?? throw new UnknownPersonException($"Unknown person {personId}.");

                if (command.supportUpdateOrganisation.Organisation.Approved)
                {
                    organisation.ApprovedOn = DateTimeOffset.UtcNow;
                }

                organisation.ReviewedBy = person;
                organisation.ReviewComment = command.supportUpdateOrganisation.Organisation.Comment;

                break;
        }

        organisationRepository.Save(organisation);

        return true;
    }
}