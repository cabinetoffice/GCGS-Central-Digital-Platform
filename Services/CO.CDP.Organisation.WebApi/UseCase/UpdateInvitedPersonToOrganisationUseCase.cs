using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateInvitedPersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IPersonInviteRepository personInviteRepository
   )
    : IUseCase<(Guid organisationId, Guid personInviteId, UpdatePersonToOrganisation updateInvitedPerson), bool>
{
    public async Task<bool> Execute((Guid organisationId, Guid personInviteId, UpdatePersonToOrganisation updateInvitedPerson) command)
    {
        if (!command.updateInvitedPerson.Scopes.Any())        
            throw new EmptyPersonRoleException($"Empty Scope of Invited Person {command.personInviteId}.");
        
        var organisation = await organisationRepository.Find(command.organisationId)
                           ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var invitedPerson = await personInviteRepository.Find(command.personInviteId)
                         ?? throw new UnknownInvitedPersonException($"Unknown invited person {command.personInviteId}.");

        var personInvite = UpdatePersonInvite(command.updateInvitedPerson, invitedPerson);

        personInviteRepository.Save(personInvite);

        return await Task.FromResult(true);     
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