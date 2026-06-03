using CO.CDP.ApplicationRegistry.Persistence;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry;

/// <summary>
/// Resolves the current authenticated user's URN from the HTTP request context.
/// The URN is read from the 'sub' JWT claim populated by the Authority service.
/// Falls back to "system" when no authenticated principal is available
/// (e.g. background workers, unauthenticated requests, or tests).
/// </summary>
public class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public string UserId =>
        httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "system";
}
