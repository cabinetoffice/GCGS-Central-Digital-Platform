using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

/// <summary>
/// Represents an authorization requirement that an API key must have one of the specified scopes.
/// </summary>
public class ApiKeyScopeAuthorizationRequirement(IEnumerable<string> requiredScopes) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the collection of scopes that are required for authorization.
    /// The user must have at least one of these scopes.
    /// </summary>
    public IEnumerable<string> RequiredScopes { get; } = requiredScopes ?? throw new ArgumentNullException(nameof(requiredScopes));
}

