using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;

namespace CO.CDP.OrganisationApp;

public class FtsUrlService : IFtsUrlService
{
    private readonly string _ftsService;
    private readonly ICookiePreferencesService _cookiePreferencesService;

    public FtsUrlService(IConfiguration configuration, ICookiePreferencesService cookiePreferencesService)
    {
        var ftsService = configuration["FtsService"] ?? throw new InvalidOperationException("FtsService is not configured.");
        _ftsService = ftsService.TrimEnd('/');
        _cookiePreferencesService = cookiePreferencesService;
    }

    public string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null)
    {
        var uriBuilder = new UriBuilder(_ftsService)
        {
            Path = $"{endpoint.TrimStart('/')}"
        };

        var queryBuilder = new QueryBuilder
        {
            { "language", CultureInfo.CurrentUICulture.Name.Replace("-", "_") }
        };

        if (organisationId.HasValue)
        {
            queryBuilder.Add("organisation_id", organisationId.Value.ToString());
        }

        if (!string.IsNullOrEmpty(redirectUrl))
        {
            queryBuilder.Add("redirect_url", redirectUrl);
        }

        CookieAcceptanceValues cookiesAccepted = _cookiePreferencesService.GetValue();
        string cookiesAcceptedValue = cookiesAccepted switch
        {
            CookieAcceptanceValues.Accept => "true",
            CookieAcceptanceValues.Reject => "false",
            _ => "unknown"
        };

        queryBuilder.Add(CookieSettings.FtsHandoverParameter, cookiesAcceptedValue);

        uriBuilder.Query = queryBuilder.ToQueryString().Value;

        return uriBuilder.Uri.ToString();
    }
}
