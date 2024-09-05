using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdatePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository
    , IPersonRepository personRepository
   )
    : IUseCase<(Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePerson), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePerson) command)
    {
        if (!command.updatePerson.Scopes.Any())
            throw new EmptyPersonRoleException($"Empty Scope of Invited Person {command.personId}.");

        var organisationPerson = await organisationRepository.FindOrganisationPerson(command.organisationId, command.personId)
          ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId} or Person {command.personId}.");


        organisationPerson.Scopes = command.updatePerson.Scopes;

        organisationRepository.SaveOrganisationPerson(organisationPerson);


        return await Task.FromResult(true);
    }
}