using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationSync;
using CO.CDP.Person.WebApi.Features;
using CO.CDP.Person.WebApi.Model;
using Microsoft.FeatureManagement;
using IOrganisationRepository = CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository;
using UmPartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.Person.WebApi.UseCase;

public class ClaimPersonInviteUseCase(
    IPersonRepository personRepository,
    IPersonInviteRepository personInviteRepository,
    IOrganisationRepository organisationRepository,
    IFeatureManager featureManager,
    IAtomicScope atomicScope,
    IOrganisationMembershipSync membershipSync,
    ILogger<ClaimPersonInviteUseCase> logger)
    : IUseCase<(Guid personId, ClaimPersonInvite claimPersonInvite), bool>
{
    public async Task<bool> Execute((Guid personId, ClaimPersonInvite claimPersonInvite) command)
    {
        var person = await personRepository.Find(command.personId)
            ?? throw new UnknownPersonException($"Unknown person {command.personId}.");
        var personInvite = await personInviteRepository.Find(command.claimPersonInvite.PersonInviteId)
            ?? throw new UnknownPersonInviteException($"Unknown personInvite {command.claimPersonInvite.PersonInviteId}.");

        GuardPersonInviteExpired(personInvite);
        GuardPersonInviteAlreadyClaimed(personInvite);
        await GuardFromDuplicateEmailWithinOrganisation(organisationId: personInvite.Organisation!.Guid, person.Email);

        var organisation = await organisationRepository.FindIncludingTenantByOrgId(personInvite.OrganisationId)
            ?? throw new UnknownOrganisationException(
                $"Unknown organisation {personInvite.OrganisationId} for PersonInvite {command.claimPersonInvite.PersonInviteId}.");

        return await atomicScope.ExecuteAsync(async ct =>
        {
            organisation.OrganisationPersons.Add(new OrganisationPerson
            {
                Person = person,
                Scopes = personInvite.Scopes,
                OrganisationId = organisation.Id,
            });

            person.Tenants.Add(organisation.Tenant);
            personInvite.Person = person;

            personRepository.Track(person);
            personInviteRepository.Track(personInvite);

            var syncEnabled = await featureManager.IsEnabledAsync(FeatureFlags.OrganisationSyncEnabled);
            var orgPartyRoles = organisation.Roles.Select(r => (UmPartyRole)(int)r).ToList();

            return syncEnabled
                ? (await membershipSync.ClaimMembershipAsync(
                        new ClaimMembershipCommand(
                            organisation.Guid,
                            person.Guid,
                            person.UserUrn,
                            personInvite.Scopes,
                            orgPartyRoles), ct))
                    .Match(
                        onLeft: error => LogAndContinue(error),
                        onRight: _ => true)
                : true;
        });
    }

    private bool LogAndContinue(SyncError error)
    {
        logger.LogError("UM membership sync failed: {Error}", error.Message);
        return true;
    }

    private static void GuardPersonInviteExpired(PersonInvite personInvite)
    {
        if (personInvite.ExpiresOn?.UtcDateTime < DateTimeOffset.UtcNow)
            throw new PersonInviteExpiredException($"PersonInvite {personInvite.Guid} has expired.");
    }

    private static void GuardPersonInviteAlreadyClaimed(PersonInvite personInvite)
    {
        if (personInvite.Person != null)
            throw new PersonInviteAlreadyClaimedException(
                $"PersonInvite {personInvite.Guid} has already been claimed.");
    }

    private async Task GuardFromDuplicateEmailWithinOrganisation(Guid organisationId, string email)
    {
        var isEmailUnique = await organisationRepository.IsEmailUniqueWithinOrganisation(organisationId, email);
        if (!isEmailUnique)
            throw new DuplicateEmailWithinOrganisationException(
                "A user with this email address already exists for your organisation.");
    }
}
