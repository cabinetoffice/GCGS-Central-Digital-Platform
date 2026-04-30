using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Features;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationSync;
using Microsoft.FeatureManagement;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdatePersonToOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IAtomicScope atomicScope,
    IOrganisationMembershipSync membershipSync,
    IFeatureManager featureManager,
    ILogger<UpdatePersonToOrganisationUseCase> logger)
    : IUseCase<(Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePerson), bool>
{
    public Task<bool> Execute((Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePerson) command) =>
        atomicScope.ExecuteAsync(ct => UpdateAsync(command, ct));

    private async Task<bool> UpdateAsync(
        (Guid organisationId, Guid personId, UpdatePersonToOrganisation updatePerson) command,
        CancellationToken ct)
    {
        if (!command.updatePerson.Scopes.Any())
            throw new EmptyPersonRoleException($"Empty Scope of Person {command.personId}.");

        var organisationPerson = await organisationRepository.FindOrganisationPerson(command.organisationId, command.personId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId} or Person {command.personId}.");

        organisationPerson.Scopes = command.updatePerson.Scopes;
        organisationRepository.TrackOrganisationPerson(organisationPerson);

        var syncEnabled = await featureManager.IsEnabledAsync(FeatureFlags.OrganisationSyncEnabled);
        return syncEnabled
            ? (await membershipSync.UpdateMembershipScopesAsync(
                    new UpdateMembershipScopesCommand(
                        command.organisationId,
                        command.personId,
                        command.updatePerson.Scopes), ct))
                .Match(
                    onLeft: error => LogAndContinue(error, command.organisationId),
                    onRight: _ => true)
            : true;
    }

    private bool LogAndContinue(SyncError error, Guid orgGuid)
    {
        logger.LogError("UM scope sync failed for org {OrgGuid}: {Error}", orgGuid, error.Message);
        return true;
    }
}