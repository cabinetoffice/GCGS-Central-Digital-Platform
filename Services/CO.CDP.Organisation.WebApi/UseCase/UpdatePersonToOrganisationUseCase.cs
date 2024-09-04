using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdatePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonRepository personRepository
   )
    : IUseCase<(Guid organisationId, Guid personInviteId, UpdatePersonToOrganisation updateInvitedPerson), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid personInviteId, UpdatePersonToOrganisation updateInvitedPerson) command)
    {
        if (!command.updateInvitedPerson.Scopes.Any())        
            throw new EmptyPersonRoleException($"Empty Scope of Invited Person {command.personInviteId}.");
        
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var organisationPerson = await personRepository.Find(command.personInviteId)
                         ?? throw new UnknownInvitedPersonException($"Unknown invited person {command.personInviteId}.");

        var person = UpdatePersonInvite(command.updateInvitedPerson, organisationPerson);


        personRepository.Save(person);

        return personInvite;
    }

    private OrganisationPerson UpdatePersonInvite(
        UpdatePersonToOrganisation command,
        OrganisationPerson person
    )
    {
        person.= command.Scopes;

        return invitedPerson;
    }
}