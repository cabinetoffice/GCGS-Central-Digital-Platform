using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;
using System.Web;

namespace CO.CDP.OrganisationApp;

public class FtsUrlService : IFtsUrlService
{
    private readonly string _ftsService;

    public FtsUrlService(IConfiguration configuration)
    {
        var ftsService = configuration["FtsService"] ?? throw new InvalidOperationException("FtsService is not configured.");
        _ftsService = ftsService.TrimEnd('/');
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

        uriBuilder.Query = queryBuilder.ToQueryString().Value;

        return uriBuilder.Uri.ToString();
    }
}

public interface IFtsUrlService
{
    string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null);
}