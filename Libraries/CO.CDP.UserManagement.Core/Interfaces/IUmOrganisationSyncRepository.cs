using CO.CDP.Functional;

namespace CO.CDP.UserManagement.Core.Interfaces;

/// <summary>
/// Keeps the User Management tables in sync when OI membership events occur.
/// All methods track changes only — <c>SaveChangesAsync</c> is deferred to the caller.
/// Returns <see cref="Result{TError,TValue}"/> — never throws for expected failures.
/// </summary>
public interface IUmOrganisationSyncRepository
{
    Task<Result<string, Unit>> EnsureCreatedAsync(
        Guid cdpGuid,
        string name,
        CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureActiveApplicationsEnabledAsync(
        Guid cdpGuid, CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureNameSyncedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureFounderOwnerCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureMemberCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyList<string> inviteScopes,
        CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureMemberScopesUpdatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        IReadOnlyList<string> newScopes,
        CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureMemberScopesAndAppRolesUpdatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        IReadOnlyList<string> newScopes,
        CancellationToken cancellationToken = default);

    Task<Result<string, Unit>> EnsureMemberRemovedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        CancellationToken cancellationToken = default);
}