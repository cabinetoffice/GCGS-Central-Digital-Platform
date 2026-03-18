namespace CO.CDP.UserManagement.Core.Interfaces;

using CO.CDP.Functional;
using PartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

/// <summary>
/// Keeps the User Management tables in sync when OI membership events occur.
/// All methods track changes only — <c>SaveChangesAsync</c> is deferred to the caller
/// (typically <c>IAtomicScope</c>) so OI and UM writes can be committed atomically.
/// Returns <see cref="Result{TError,TValue}"/> — never throws for expected failures.
/// </summary>
public interface IUmOrganisationSyncRepository
{
    Task<Result<string, Unit>> EnsureCreatedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureActiveApplicationsEnabledAsync(
        Guid cdpGuid, CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureNameSyncedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureFounderOwnerCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyCollection<PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureMemberCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyList<string> inviteScopes,
        IReadOnlyCollection<PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken = default);
}
