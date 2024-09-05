using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.UseCase;

public class ClaimPersonInviteUseCase(
    IPersonRepository personRepository,
    IPersonInviteRepository personInviteRepository)
    : IUseCase<(Guid personId, ClaimPersonInvite claimPersonInvite), PersonInvite>
{
    public async Task<PersonInvite> Execute((Guid personId, ClaimPersonInvite claimPersonInvite) command)
    {
        var person = await personRepository.Find(command.personId) ?? throw new Exception($"Unknown person {command.personId}.");
        var personInvite = await personInviteRepository.Find(command.claimPersonInvite.PersonInviteId) ?? throw new Exception($"Unknown personInvite {command.claimPersonInvite.PersonInviteId}.");

        if (personInvite.Person != null)
        {
            throw new PersonInviteAlreadyClaimedException(
                $"PersonInvite {command.claimPersonInvite.PersonInviteId} has already been claimed.");
        }

        var organisation = personInvite.Organisation;
        organisation.OrganisationPersons.Add(new OrganisationPerson
        {
            Person = person,
            Organisation = personInvite.Organisation,
            Scopes = personInvite.Scopes
        });

        personInvite.Person = person;

        personRepository.Save(person);
        personInviteRepository.Save(personInvite);

        return personInvite;
    }
}