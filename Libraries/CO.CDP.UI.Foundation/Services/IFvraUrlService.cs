namespace CO.CDP.UI.Foundation.Services;

public interface IFvraUrlService
{
    /// <summary>
    /// Builds a URL to the FVRA service endpoint with optional additional query parameters
    /// </summary>
    /// <param name="endpoint">The endpoint path</param>
    /// <param name="organisationId">Optional organisation ID</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance status</param>
    /// <param name="additionalParams">Additional query parameters to include</param>
    /// <returns>The complete URL to the FVRA service endpoint</returns>
    string BuildUrl(string endpoint, Guid? organisationId, string? redirectUri, bool? cookieAcceptance, Dictionary<string, string?>? additionalParams);
}