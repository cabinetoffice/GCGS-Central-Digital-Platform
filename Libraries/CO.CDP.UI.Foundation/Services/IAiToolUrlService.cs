namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Interface for building URLs to the AI Tool service
/// </summary>
public interface IAiToolUrlService
{
    /// <summary>
    /// Builds a URL to an AI Tool service endpoint with optional additional query parameters
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance status</param>
    /// <param name="additionalParams">Additional query parameters to include</param>
    /// <returns>The complete URL to the AI Tool service endpoint</returns>
    string BuildUrl(string endpoint, Guid? organisationId, string? redirectUri, bool? cookieAcceptance, Dictionary<string, string?>? additionalParams);
}
