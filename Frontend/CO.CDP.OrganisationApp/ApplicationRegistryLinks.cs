namespace CO.CDP.OrganisationApp;

/// <summary>
/// Links into the DAPS Application Registry web app, which hosts the application catalogue
/// (all users) and the organisation registry dashboard (organisation ADMINs). It signs the
/// user in with the same GOV.UK One Login session and reads their CDP organisations and
/// organisation role from cdp_claims, so SIRSI only has to hand over.
/// </summary>
public static class ApplicationRegistryLinks
{
    public const string WebUrlKey = "ApplicationRegistry:WebUrl";

    /// <summary>
    /// The registry landing URL, optionally pinned to a CDP organisation; null when the
    /// registry URL is not configured. Admins of that organisation land on its dashboard,
    /// everyone else on its application catalogue.
    /// </summary>
    public static string? Build(IConfiguration configuration, Guid? organisationId = null)
    {
        var baseUrl = configuration[WebUrlKey];
        if (string.IsNullOrWhiteSpace(baseUrl) ||
            !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return null;
        }

        var url = baseUrl.TrimEnd('/') + "/";
        return organisationId is { } id ? $"{url}?cdp_organisation_id={id}" : url;
    }
}
