using System.Text.RegularExpressions;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class SearchModel(IOrganisationClient organisationClient,
    ISession session,ILogger<SearchModel> logger) : LoggedInUserAwareModel(session)
{
    private readonly ILogger<SearchModel> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public string? Title { get; set; }

    public string? SearchTitle { get; set; }

    public int TotalOrganisations { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int Skip { get; set; }

    public string? ErrorMessage { get; set; }

    private bool IsFromSamePage = false;

    public IList<OrganisationSearchByPponResult> Organisations { get; set; } = [];

    public async Task<IActionResult> OnGet(string type, int pageNumber = 1,
        [FromQuery(Name = "q")] string? searchText = null,
        [FromQuery(Name = "sortOrder")] string? sortOrder = "relevance")
    {
        InitModel(type, pageNumber);
        await GetResults(searchText, sortOrder);
        return Page();
    }

    private void InitModel(string type, int pageNumber)
    {
        PageSize = 10;
        if (pageNumber < 1) pageNumber = 1;
        Title = StaticTextResource.PponSearch_Title;
        SearchTitle = StaticTextResource.PponSearch_Hint;
        Skip = (pageNumber - 1) * PageSize;
        CurrentPage = pageNumber;

        string? refererUrl = Request.Headers.Referer.ToString();
        IsFromSamePage = !string.IsNullOrEmpty(refererUrl) &&
                         refererUrl.Contains("organisation/buyer/search", StringComparison.OrdinalIgnoreCase);

    }

    private async Task GetResults(string? searchText, string? sortOrder)
    {
        if (!IsFromSamePage) {
            resetKeyParams();
            return;
        }

        if (string.IsNullOrWhiteSpace(searchText)) {
            ErrorMessage = StaticTextResource.PponSearch_Invalid_Search_Value;
            resetKeyParams();
            return;
        }

        string regexPattern = @"[^a-zA-Z0-9\s\-]";
        string originalSearchText = searchText.Trim();
        bool containsInvalidChars = Regex.IsMatch(originalSearchText, regexPattern);
        string cleanedSearchText = Regex.Replace(originalSearchText, regexPattern, string.Empty);

        if (string.IsNullOrWhiteSpace(cleanedSearchText) || containsInvalidChars) {
            ErrorMessage = StaticTextResource.PponSearch_Invalid_Search_Value;
            resetKeyParams();
            return;
        }

        try
        {
            var orgs = await OrganisationClientExtensions.SearchOrganisationByNameOrPpon(
                organisationClient, cleanedSearchText, PageSize, Skip);

            if (orgs.Count == 0)
            {
                ErrorMessage = StaticTextResource.PponSearch_NoResults;
                resetKeyParams();
                return;
            }

            Organisations = sortOrder switch
            {
                "ascending" => orgs.OrderBy(o => o.Name).ToList(),
                "descending" => orgs.OrderByDescending(o => o.Name).ToList(),
                _ => orgs.ToList()
            };
            ErrorMessage = null;
            TotalOrganisations = orgs.Count;
            TotalPages = (int)Math.Ceiling((double)TotalOrganisations / PageSize);
        }
        catch (Exception ex) when (
            (ex is ApiException apiEx && apiEx.StatusCode != 404) ||
            (ex is HttpRequestException httpEx && httpEx.StatusCode != System.Net.HttpStatusCode.NotFound) ||
            (!(ex is ApiException) && !(ex is HttpRequestException))) {
            LogApiError(ex);
        }
    }

    private void resetKeyParams()
    {
        Organisations = [];
        TotalOrganisations = 0;
        TotalPages = 0;
    }

    public string FormatAddresses(IEnumerable<OrganisationAddress> addresses)
    {
        if (addresses == null || !addresses.Any())
            return "N/A";
        var address = addresses.FirstOrDefault();
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(address.StreetAddress))
            parts.Add(address.StreetAddress);
        if (!string.IsNullOrWhiteSpace(address.Locality))
            parts.Add(address.Locality);
        if (!string.IsNullOrWhiteSpace(address.Region))
            parts.Add(address.Region);
        if (!string.IsNullOrWhiteSpace(address.PostalCode))
            parts.Add(address.PostalCode);
        if (!string.IsNullOrWhiteSpace(address.Country))
            parts.Add(address.Country);
        return parts.Any() ? string.Join(", ", parts) : "N/A";
    }

    public string GetTagClassForRole(PartyRole role)
    {
        return role switch
        {
            PartyRole.Buyer => "govuk-tag--blue",
            PartyRole.ProcuringEntity => "govuk-tag--green",
            PartyRole.Supplier => "govuk-tag--purple",
            PartyRole.Tenderer => "govuk-tag--yellow",
            PartyRole.Funder => "govuk-tag--red",
            PartyRole.Enquirer => "govuk-tag--orange",
            PartyRole.Payer => "govuk-tag--pink",
            PartyRole.Payee => "govuk-tag--grey",
            PartyRole.ReviewBody => "govuk-tag--turquoise",
            PartyRole.InterestedParty => "govuk-tag--light-blue",
            _ => "govuk-tag--black"
        };
    }
    private void LogApiError(Exception ex)
    {
        var errorMessage = "Error occurred while searching for organisations";
        var cdpException = new CdpExceptionLogging(errorMessage, "SEARCH_REGISTRY_OF_PPON_ERROR", ex);
        _logger.LogError(cdpException, errorMessage);
    }
}