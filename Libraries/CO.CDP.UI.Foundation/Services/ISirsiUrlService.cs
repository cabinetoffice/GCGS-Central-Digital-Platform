using System;

namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Interface for building URLs to the Sirsi service
/// </summary>
public interface ISirsiUrlService
{
    /// <summary>
    /// Builds a URL to a Sirsi service endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance override</param>
    /// <returns>The complete URL to the Sirsi service endpoint</returns>
    string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUri = null, bool? cookieAcceptance = null);

    /// <summary>
    /// Builds an authenticated URL to a Sirsi service endpoint using the one-login sign-in flow
    /// </summary>
    /// <param name="endpoint">The endpoint path to redirect to after authentication</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance override</param>
    /// <returns>The complete URL with one-login authentication redirect</returns>
    string BuildAuthenticatedUrl(string endpoint, Guid? organisationId = null, bool? cookieAcceptance = null);

    /// <summary>
    /// Returns the path for the given endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <returns>The path for the given endpoint</returns>
    string GetPath(string endpoint);
}