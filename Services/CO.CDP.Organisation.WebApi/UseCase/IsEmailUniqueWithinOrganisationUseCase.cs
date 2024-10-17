using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class IsEmailUniqueWithinOrganisationUseCase(IOrganisationRepository organisationRepository)
    : IUseCase<(Guid organisationId, string personEmail), bool>
{
    public async Task<bool> Execute((Guid organisationId, string personEmail) command)
    {
        var isUnique = await organisationRepository.IsEmailUniqueWithinOrganisation(command.organisationId, command.personEmail);

        return isUnique;
    }
}