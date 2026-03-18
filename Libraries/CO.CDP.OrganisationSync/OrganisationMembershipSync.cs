namespace CO.CDP.OrganisationSync;

using CO.CDP.Functional;
using CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Thin facade over <see cref="IUmOrganisationSyncRepository"/> that pipelines Result types
/// and maps UM string errors to <see cref="SyncError"/>.
/// Does not call SaveChangesAsync — changes are deferred to <see cref="IAtomicScope"/>.
/// </summary>
public sealed class OrganisationMembershipSync(
    IUmOrganisationSyncRepository umRepo) : IOrganisationMembershipSync
{
    public Task<Result<SyncError, MembershipSynced>> ClaimMembershipAsync(
        ClaimMembershipCommand command,
        CancellationToken ct = default) =>
        umRepo.EnsureMemberCreatedAsync(
            command.OrganisationGuid,
            command.PersonGuid,
            command.UserPrincipalId,
            command.InviteScopes,
            command.OrganisationPartyRoles,
            ct)
        .MapToSyncResult(_ => new MembershipSynced(command.OrganisationGuid, command.PersonGuid));

    public Task<Result<SyncError, FounderSynced>> CreateFounderMembershipAsync(
        CreateFounderCommand command,
        CancellationToken ct = default) =>
        umRepo.EnsureCreatedAsync(command.OrganisationGuid, command.OrganisationName, ct)
            .BindUmResult(_ => umRepo.EnsureActiveApplicationsEnabledAsync(command.OrganisationGuid, ct))
            .BindUmResult(_ => umRepo.EnsureFounderOwnerCreatedAsync(
                command.OrganisationGuid,
                command.PersonGuid,
                command.UserPrincipalId,
                command.OrganisationPartyRoles,
                ct))
            .MapToSyncResult(_ => new FounderSynced(command.OrganisationGuid, command.PersonGuid));

    public Task<Result<SyncError, Unit>> SyncOrganisationNameAsync(
        Guid organisationGuid,
        string name,
        CancellationToken ct = default) =>
        umRepo.EnsureNameSyncedAsync(organisationGuid, name, ct)
            .MapToSyncResult(_ => Unit.Value);
}
