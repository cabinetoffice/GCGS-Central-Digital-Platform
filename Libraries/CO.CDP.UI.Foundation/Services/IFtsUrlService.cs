namespace CO.CDP.UI.Foundation.Services;

/// <summary>
/// Interface for building URLs to the FTS service.
/// </summary>
public interface IFtsUrlService
{
    /// <summary>
    /// Builds a URL to an FTS service endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint path.</param>
    /// <param name="organisationId">Optional organisation ID.</param>
    /// <param name="redirectUrl">Optional redirect URL.</param>
    /// <param name="cookieAcceptance">Optional cookie acceptance override.</param>
    /// <returns>The complete URL to the FTS service endpoint.</returns>
    string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null, bool? cookieAcceptance = null);

    /// <summary>
    /// Returns the path for the given endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint path.</param>
    /// <returns>The path for the given endpoint.</returns>
    string GetPath(string endpoint);
}