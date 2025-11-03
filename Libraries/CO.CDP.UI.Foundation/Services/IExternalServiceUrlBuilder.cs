namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Interface for building URLs to external services
/// </summary>
public interface IExternalServiceUrlBuilder
{
    /// <summary>
    /// Builds a URL to an external service endpoint with optional additional query parameters
    /// </summary>
    /// <param name="service">The external service to build a URL for</param>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance status</param>
    /// <param name="additionalParams">Additional query parameters to include</param>
    /// <returns>The complete URL to the external service endpoint</returns>
    string BuildUrl(ExternalService service, string endpoint, Guid? organisationId = null,
                   string? redirectUri = null, bool? cookieAcceptance = null,
                   Dictionary<string, string?>? additionalParams = null);
}
