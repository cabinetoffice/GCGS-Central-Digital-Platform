using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdatePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonRepository personRepository
   )
    : IUseCase<(Guid organisationId, Guid personInviteId, UpdatePersonToOrganisation updateInvitedPerson), PersonInvite>
{
    public async Task<PersonInvite> Execute((Guid organisationId, Guid personInviteId, UpdatePersonToOrganisation updateInvitedPerson) command)
    {
        if (!command.updateInvitedPerson.Scopes.Any())        
            throw new EmptyPersonRoleException($"Empty Scope of Invited Person {command.personInviteId}.");
        
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var invitedPerson = await personRepository.Find(command.personInviteId)
                         ?? throw new UnknownInvitedPersonException($"Unknown invited person {command.personInviteId}.");

        var personInvite = UpdatePersonInvite(command.updateInvitedPerson, invitedPerson);


        personRepository.Save(personInvite);

        return personInvite;
    }

    private PersonInvite UpdatePersonInvite(
        UpdatePersonToOrganisation command,
        PersonInvite invitedPerson
    )
    {
        invitedPerson.Scopes = command.Scopes;

        return invitedPerson;
    }
}