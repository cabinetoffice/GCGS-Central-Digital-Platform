using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Features;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationSync;
using Microsoft.FeatureManagement;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class RemovePersonFromOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IAtomicScope atomicScope,
    IOrganisationMembershipSync membershipSync,
    IFeatureManager featureManager,
    ILogger<RemovePersonFromOrganisationUseCase> logger)
    : IUseCase<(Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation), bool>
{
    public Task<bool> Execute((Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation) command) =>
        atomicScope.ExecuteAsync(ct => RemoveAsync(command, ct));

    private async Task<bool> RemoveAsync(
        (Guid organisationId, RemovePersonFromOrganisation removePersonFromOrganisation) command,
        CancellationToken ct)
    {
        var organisation = await organisationRepository.FindIncludingPersons(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var organisationPerson = organisation.OrganisationPersons.FindLast(op => op.Person.Guid == command.removePersonFromOrganisation.PersonId);
        var personWithTenant = organisation.Tenant.Persons.FindLast(tp => tp.Guid == command.removePersonFromOrganisation.PersonId);

        if (organisationPerson == null && personWithTenant == null) return false;

        if (personWithTenant != null)
            organisation.Tenant.Persons.Remove(personWithTenant);

        if (organisationPerson != null)
            organisation.OrganisationPersons.Remove(organisationPerson);

        organisationRepository.Track(organisation);

        var syncEnabled = await featureManager.IsEnabledAsync(FeatureFlags.OrganisationSyncEnabled);
        return syncEnabled
            ? (await membershipSync.RemoveMembershipAsync(
                    new RemoveMembershipCommand(
                        command.organisationId,
                        command.removePersonFromOrganisation.PersonId), ct))
                .Match(
                    onLeft: error => LogAndContinue(error, command.organisationId),
                    onRight: _ => true)
            : true;
    }

    private bool LogAndContinue(SyncError error, Guid orgGuid)
    {
        logger.LogError("UM member removal sync failed for org {OrgGuid}: {Error}", orgGuid, error.Message);
        return true;
    }
}