using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.MQ;
using CO.CDP.Person.WebApi.Events;
using CO.CDP.Person.WebApi.Model;

namespace CO.CDP.Person.WebApi.UseCase;

public class ClaimPersonInviteUseCase(
    IPersonRepository personRepository,
    IPersonInviteRepository personInviteRepository,
    IOrganisationRepository organisationRepository,
    IPublisher? publisher = null)
    : IUseCase<(Guid personId, ClaimPersonInvite claimPersonInvite), bool>
{
    public async Task<bool> Execute((Guid personId, ClaimPersonInvite claimPersonInvite) command)
    {
        var person = await personRepository.Find(command.personId) ?? throw new UnknownPersonException($"Unknown person {command.personId}.");
        var personInvite = await personInviteRepository.Find(command.claimPersonInvite.PersonInviteId) ?? throw new UnknownPersonInviteException($"Unknown personInvite {command.claimPersonInvite.PersonInviteId}.");

        GuardPersonInviteExpired(personInvite);
        GuardPersonInviteAlreadyClaimed(personInvite);
        await GuardFromDuplicateEmailWithinOrganisation(organisationId: personInvite.Organisation!.Guid, person.Email);

        var organisation = await organisationRepository.FindIncludingTenantByOrgId(personInvite.OrganisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {personInvite.OrganisationId} for PersonInvite {command.claimPersonInvite.PersonInviteId}."); ;

        organisation.OrganisationPersons.Add(new OrganisationPerson
        {
            Person = person,
            Scopes = personInvite.Scopes,
            OrganisationId = organisation.Id,
        });

        person.Tenants.Add(organisation.Tenant);

        personInvite.Person = person;

        personRepository.Save(person);
        personInviteRepository.Save(personInvite);
        if (publisher != null)
        {
            await publisher.Publish(new PersonInviteClaimed
            {
                PersonInviteGuid = personInvite.Guid,
                PersonGuid = person.Guid,
                UserUrn = person.UserUrn,
                OrganisationGuid = organisation.Guid
            });
        }

        return true;
    }

    private void GuardPersonInviteExpired(PersonInvite personInvite)
    {
        if (personInvite.ExpiresOn?.UtcDateTime < DateTimeOffset.UtcNow)
        {
            throw new PersonInviteExpiredException(
                $"PersonInvite {personInvite.Guid} has expired.");
        }
    }

    private void GuardPersonInviteAlreadyClaimed(PersonInvite personInvite)
    {
        if (personInvite.Person != null)
        {
            throw new PersonInviteAlreadyClaimedException(
                $"PersonInvite {personInvite.Guid} has already been claimed.");
        }
    }

    private async Task GuardFromDuplicateEmailWithinOrganisation(Guid organisationId, string email)
    {
        var isEmailUnique = await organisationRepository.IsEmailUniqueWithinOrganisation(organisationId, email);
        if (!isEmailUnique)
        {
            throw new DuplicateEmailWithinOrganisationException($"A user with this email address already exists for your organisation.");
        }
    }
}
