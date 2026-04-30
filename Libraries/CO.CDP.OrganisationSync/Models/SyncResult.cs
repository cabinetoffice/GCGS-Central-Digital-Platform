namespace CO.CDP.OrganisationSync;

public abstract record SyncError(string Message);

public sealed record OrganisationNotFoundError(Guid Guid)
    : SyncError($"UM organisation {Guid} not found.");

public sealed record RoleResolutionError(IReadOnlyList<string> Scopes)
    : SyncError($"Could not resolve UM role from scopes: [{string.Join(", ", Scopes)}].");

public sealed record SyncFailureError(string Message, Exception? Inner = null)
    : SyncError(Message);

public sealed record MembershipSynced(Guid OrganisationGuid, Guid PersonGuid);

public sealed record FounderSynced(Guid OrganisationGuid, Guid PersonGuid);
