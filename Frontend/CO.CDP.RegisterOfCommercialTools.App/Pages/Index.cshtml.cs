using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages;

public class IndexModel(ISearchService searchService, ISirsiUrlService sirsiUrlService, IFtsUrlService ftsUrlService, ILogger<IndexModel> logger)
    : PageModel
{
    [BindProperty(SupportsGet = true, Name = "sort")] public SearchModel SearchParams { get; set; } = new();

    [BindProperty(SupportsGet = true, Name = "acc")]
    public List<string> OpenAccordions { get; set; } = new();

    public List<SearchResult> SearchResults { get; set; } = [];
    public PaginationPartialModel? Pagination { get; set; }

    [BindProperty(SupportsGet = true, Name = "pageNumber")]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true, Name = "origin")]
    public string? Origin { get; set; }

    [BindProperty(SupportsGet = true, Name = "organisation_id")]
    public Guid? OrganisationId { get; set; }

    private const int PageSize = 10;

    public string HomeUrl { get; private set; } = string.Empty;

    public int TotalCount { get; set; }
    public string? ScriptNonce => HttpContext.Items["ContentSecurityPolicyNonce"] as string;

    public async Task OnGetAsync()
    {
        logger.LogInformation("Processing search request: Page {PageNumber}, Keywords: {Keywords}, Status: [{Status}]",
            PageNumber, SearchParams.Keywords, string.Join(", ", SearchParams.Status));

        try
        {
            SetHomeUrl();

            if (!Request.Query.ContainsKey("acc") && !OpenAccordions.Any())
            {
                OpenAccordions =
                [
                    "commercial-tool", "commercial-tool-status", "contracting-authority-usage", "award-method", "fees",
                    "date-range"
                ];
            }

            var (results, totalCount) = await searchService.SearchAsync(SearchParams, PageNumber, PageSize);

            SearchResults = results.Select(result => result with
            {
                Url = !string.IsNullOrEmpty(result.Id) && result.Id != "Unknown" ? ftsUrlService.BuildUrl($"/procurement/{result.Id}") : null
            }).ToList();
            TotalCount = totalCount;

            logger.LogInformation("Search completed successfully. Found {TotalCount} results for page {PageNumber}",
                totalCount, PageNumber);

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
            HomeUrl = sirsiUrlService.BuildUrl($"/organisation/{OrganisationId}/buyer");
        }
        else
        {
            HomeUrl = ftsUrlService.BuildUrl("/Search");
        }
    }
}