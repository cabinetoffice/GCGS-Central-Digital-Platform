using System.Globalization;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages;

public class IndexModel(
    ISearchService searchService,
    ISirsiUrlService sirsiUrlService,
    IFtsUrlService ftsUrlService,
    ICpvCodeService cpvCodeService,
    ILocationCodeService locationCodeService,
    ILogger<IndexModel> logger)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public SearchModel SearchParams { get; set; } = new();

    [BindProperty(SupportsGet = true, Name = "acc")]
    public List<string> OpenAccordions { get; set; } = new();

    public List<SearchResult> SearchResults { get; set; } = [];
    public PaginationPartialModel? Pagination { get; set; }

    public CpvCodeSelection CpvSelection { get; set; } = new();
    public LocationCodeSelection LocationSelection { get; set; } = new();

    [BindProperty(SupportsGet = true, Name = "pageNumber")]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true, Name = "origin")]
    public string? Origin { get; set; }

    [BindProperty(SupportsGet = true, Name = "organisation_id")]
    public Guid? OrganisationId { get; set; }

    [BindProperty(SupportsGet = true, Name = "cookies_accepted")]
    public string? CookiesAccepted { get; set; }

    private const int PageSize = 20;

    public string HomeUrl { get; private set; } = string.Empty;

    public int TotalCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SetHomeUrl();

        try
        {
            await SetCpvAndLocationCodes();

            if (ShouldRedirectToCleanQueryParams())
            {
                return RedirectWithCleanedQueryParams(["open_frameworks", "utilities_only"]);
            }

            var searchResult = await searchService.SearchAsync(SearchParams, PageNumber, PageSize);

            return searchResult.Match(
                error =>
                {
                    logger.LogError("Search API call failed: ErrorType={ErrorType}, Message={Message}",
                        error.GetType().Name, error.Message);
                    return RedirectToPage("/error");
                },
                ((List<SearchResult>, int) success) =>
                {
                    var (results, totalCount) = success;
                    SearchResults = results;
                    TotalCount = totalCount;

                    Pagination = new PaginationPartialModel
                    {
                        CurrentPage = PageNumber,
                        PageSize = PageSize,
                        TotalItems = totalCount,
                        Url = Request.QueryString.HasValue ? Request.Path + Request.QueryString : Request.Path
                    };

                    return (IActionResult)Page();
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing search request: Page {PageNumber}, Keywords: {Keywords}",
                PageNumber, SearchParams.Keywords);
            return RedirectToPage("/error");
        }
    }

    private bool ShouldRedirectToCleanQueryParams()
    {
        var query = Request.Query;
        var hasOpenFrameworks = query.ContainsKey("open_frameworks") && query["open_frameworks"] == "true";
        var hasUtilitiesOnly = query.ContainsKey("utilities_only") && query["utilities_only"] == "true";
        var hasFilterFrameworks = query.ContainsKey("filter_frameworks") && query["filter_frameworks"] == "true";
        var hasFilterMarkets = query.ContainsKey("filter_markets") && query["filter_markets"] == "true";

        return (hasOpenFrameworks && !hasFilterFrameworks) || (hasUtilitiesOnly && !hasFilterMarkets);
    }

    private RedirectToPageResult RedirectWithCleanedQueryParams(string[] paramsToRemove)
    {
        var queryString = Request.QueryString.Value ?? string.Empty;
        var cleanedParams = QueryHelpers.ParseQuery(queryString);

        foreach (var param in paramsToRemove)
        {
            cleanedParams.Remove(param);
        }

        return RedirectToPage("/Index", cleanedParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()));
    }

    private void SetHomeUrl()
    {
        if (Origin == "buyer-view" && OrganisationId.HasValue)
        {
            HomeUrl = sirsiUrlService.BuildAuthenticatedUrl($"/organisation/{OrganisationId}/buyer", OrganisationId);
        }
        else if (HttpContext.User.Identity?.IsAuthenticated == true)
        {
            HomeUrl = ftsUrlService.BuildUrl("/login", OrganisationId, redirectUrl: "/Search");
        }
        else
        {
            HomeUrl = ftsUrlService.BuildUrl("/Search", OrganisationId);
        }
    }


    private async Task SetCpvAndLocationCodes()
    {
        CpvSelection = new CpvCodeSelection
        {
            SelectedCodes = SearchParams.CpvCodes
        };

        if (SearchParams.CpvCodes.Any())
        {
            var selectedCpvCodes = await cpvCodeService.GetByCodesAsync(SearchParams.CpvCodes);
            CpvSelection.SelectedItems.AddRange(selectedCpvCodes);
        }

        LocationSelection = new LocationCodeSelection
        {
            SelectedCodes = SearchParams.LocationCodes
        };

        if (SearchParams.LocationCodes.Any())
        {
            var selectedLocationCodes = await locationCodeService.GetByCodesAsync(SearchParams.LocationCodes);
            LocationSelection.SelectedItems.AddRange(selectedLocationCodes);
        }
    }

    public ContextParams GetContextParams() => new()
    {
        Language = CultureInfo.CurrentUICulture.Name.Replace('-', '_'),
        Origin = Origin,
        OrganisationId = OrganisationId,
        CookiesAccepted = CookiesAccepted
    };
}