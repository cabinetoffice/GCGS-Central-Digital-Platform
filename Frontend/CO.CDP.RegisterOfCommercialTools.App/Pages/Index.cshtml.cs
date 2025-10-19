using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    private const int PageSize = 20;

    public string HomeUrl { get; private set; } = string.Empty;

    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
        logger.LogInformation(
            "Processing search request: Page {PageNumber}, Keywords: {Keywords}, Status: [{Status}], CpvCodes: [{CpvCodes}]",
            PageNumber, SearchParams.Keywords, string.Join(", ", SearchParams.Status),
            string.Join(", ", SearchParams.CpvCodes));

        try
        {
            SetHomeUrl();

            if (!Request.Query.ContainsKey("acc") && !OpenAccordions.Any())
            {
                OpenAccordions =
                [
                    "commercial-tool", "commercial-tool-status", "contracting-authority-usage", "award-method",
                    "industry-cpv-code", "contract-location", "fees", "date-range"
                ];
            }

            await PopulateCodeSelections();

            var (results, totalCount) = await searchService.SearchAsync(SearchParams, PageNumber, PageSize);

            SearchResults = results;
            TotalCount = totalCount;

            logger.LogInformation("Search completed successfully. Found {TotalCount} results.",
                totalCount);

            Pagination = new PaginationPartialModel
            {
                CurrentPage = PageNumber,
                PageSize = PageSize,
                TotalItems = totalCount,
                Url = Request.QueryString.HasValue ? Request.Path + Request.QueryString : Request.Path
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing search request: Page {PageNumber}, Keywords: {Keywords}",
                PageNumber, SearchParams.Keywords);
            throw;
        }
    }

    private void SetHomeUrl()
    {
        if (Origin == "buyer-view" && OrganisationId.HasValue)
        {
            HomeUrl = sirsiUrlService.BuildAuthenticatedUrl($"/organisation/{OrganisationId}/buyer", OrganisationId);
        }
        else
        {
            HomeUrl = ftsUrlService.BuildUrl("/Search");
        }
    }

    private async Task PopulateCodeSelections()
    {
        CpvSelection = new CpvCodeSelection
        {
            SelectedCodes = SearchParams.CpvCodes
        };

        if (SearchParams.CpvCodes.Any())
        {
            var selectedCpvCodes = await cpvCodeService.GetByCodesAsync(SearchParams.CpvCodes);
            foreach (var cpvCode in selectedCpvCodes)
            {
                CpvSelection.AddSelection(cpvCode.Code, cpvCode.DescriptionEn, cpvCode.DescriptionCy);
            }
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
}