namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Interface for building URLs to the Commercial Tools service
/// </summary>
public interface ICommercialToolsUrlService
{
    /// <summary>
    /// Builds a URL to a Commercial Tools service endpoint with optional additional query parameters
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUrl">Optional redirect URL</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance status</param>
    /// <param name="additionalParams">Additional query parameters to include</param>
    /// <returns>The complete URL to the Commercial Tools service endpoint</returns>
    string BuildUrl(string endpoint, Guid? organisationId, string? redirectUrl, bool? cookieAcceptance, Dictionary<string, string?>? additionalParams);
}
