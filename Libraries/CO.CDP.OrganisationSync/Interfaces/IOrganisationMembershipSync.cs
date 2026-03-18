namespace CO.CDP.OrganisationSync;

using CO.CDP.Functional;

/// <summary>
/// UM-side sync operations. Returns <see cref="Result{TError,TValue}"/> — never throws for expected failures.
/// Does not call <c>SaveChangesAsync</c> — changes are tracked only; <see cref="IAtomicScope"/> flushes them.
/// </summary>
public interface IOrganisationMembershipSync
{
    /// <summary>
    /// Tracks a new UM membership for a person who has claimed an OI invite.
    /// Idempotent — returns success if membership already exists.
    /// </summary>
    Task<Result<SyncError, MembershipSynced>> ClaimMembershipAsync(
        ClaimMembershipCommand command,
        CancellationToken ct = default);

    /// <summary>
    /// Tracks creation of the UM organisation row, default application enablement,
    /// and the founder Owner membership.
    /// Idempotent — skips steps that already exist.
    /// </summary>
    Task<Result<SyncError, FounderSynced>> CreateFounderMembershipAsync(
        CreateFounderCommand command,
        CancellationToken ct = default);

    /// <summary>
    /// Keeps the UM organisation name in sync with the OI organisation name.
    /// Idempotent — creates the UM org if it doesn't exist yet.
    /// </summary>
    Task<Result<SyncError, Unit>> SyncOrganisationNameAsync(
        Guid organisationGuid,
        string name,
        CancellationToken ct = default);
}
