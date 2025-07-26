using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.UI.Foundation.Services;
using CO.CDP.UI.Foundation.Utilities;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages;

public class IndexModel(ISearchService searchService, ISirsiUrlService sirsiUrlService)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public SearchModel SearchParams { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public List<string> OpenAccordions { get; set; } =
    [
        "commercial-tool", "commercial-tool-status", "contracting-authority-usage", "award-method", "fees",
        "date-range"
    ];

    public List<SearchResult> SearchResults { get; set; } = [];
    public PaginationPartialModel? Pagination { get; set; }

    [BindProperty(SupportsGet = true, Name = "pageNumber")]
    public int PageNumber { get; set; } = 1;

    private const int PageSize = 10;

    public string SirsiHomeUrl { get; private set; } = sirsiUrlService.BuildUrl("/");

    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
        var (results, totalCount) = await searchService.SearchAsync(SearchParams, PageNumber, PageSize);

        SearchResults = results;
        TotalCount = totalCount;

        Pagination = new PaginationPartialModel
        {
            CurrentPage = PageNumber,
            PageSize = PageSize,
            TotalItems = totalCount,
            Url = "/"
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        SanitiseSearchParams();

        await OnGetAsync();
        return Page();
    }
    private void SanitiseSearchParams()
    {
        SearchParams.Keywords = InputSanitiser.SanitiseSingleLineTextInput(SearchParams.Keywords);
    }


    public IActionResult OnPostReset()
    {
        return RedirectToPage();
    }
}

