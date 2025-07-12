using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.App.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages
{
    public class IndexModel(ISearchService searchService) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public SearchModel SearchParams { get; set; } = new();

        public List<SearchResult> SearchResults { get; set; } = [];
        public PaginationPartialModel? Pagination { get; set; }

        [BindProperty(SupportsGet = true, Name = "pageNumber")]
        public int PageNumber { get; set; } = 1;

        private const int PageSize = 10;

        public async Task OnGetAsync()
        {
            var (results, totalCount) = await searchService.SearchAsync(SearchParams, PageNumber, PageSize);

            SearchResults = results;

            Pagination = new PaginationPartialModel
            {
                CurrentPage = PageNumber,
                PageSize = PageSize,
                TotalItems = totalCount,
                Url = "/search"
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CheckDateBindingErrors("SearchParams.SubmissionDeadlineFrom", "Please enter a valid date");
            CheckDateBindingErrors("SearchParams.SubmissionDeadlineTo", "Please enter a valid date");
            CheckDateBindingErrors("SearchParams.ContractStartDateFrom", "Please enter a valid date");
            CheckDateBindingErrors("SearchParams.ContractStartDateTo", "Please enter a valid date");
            CheckDateBindingErrors("SearchParams.ContractEndDateFrom", "Please enter a valid date");
            CheckDateBindingErrors("SearchParams.ContractEndDateTo", "Please enter a valid date");

            await OnGetAsync();
            return Page();
        }

        private void CheckDateBindingErrors(string key, string errorMessage)
        {
            if (ModelState.TryGetValue(key, out var modelState) && modelState.Errors.Any())
            {
                modelState.Errors.Clear();

                ModelState.AddModelError(key, errorMessage);
            }
        }

        public IActionResult OnPostReset()
        {
            return RedirectToPage();
        }
    }

    public record SearchResult(
        Guid Id,
        string Title,
        string Caption,
        string Link,
        string CommercialTool,
        SearchResultStatus Status,
        string MaximumFee,
        string OtherContractingAuthorityCanUse,
        string SubmissionDeadline,
        string ContractDates,
        string AwardMethod
    );

    [FeesValidator("FeeFrom", "FeeTo", "NoFees", ErrorMessage = "Fee values cannot be provided when 'No fees' is selected")]
    public class SearchModel
    {
        public string? Keywords { get; set; }

        public string? SortOrder { get; set; }

        public string? FrameworkOptions { get; set; }

        public string? DynamicMarketOptions { get; set; }

        public string? CommercialToolStatus { get; set; }

        public string? AwardMethod { get; set; }

        [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
        public decimal? FeeFrom { get; set; }

        [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
        [DecimalRange("FeeFrom", ErrorMessage = "To must be more than from")]
        public decimal? FeeTo { get; set; }

        public string? NoFees { get; set; }

        public List<string> Status { get; set; } = [];

        public string? ContractingAuthorityUsage { get; set; }

        public DateOnly? SubmissionDeadlineFrom { get; set; }

        [DateRange("SubmissionDeadlineFrom", ErrorMessage = "To date must be after from date")]
        public DateOnly? SubmissionDeadlineTo { get; set; }

        public DateOnly? ContractStartDateFrom { get; set; }

        [DateRange("ContractStartDateFrom", ErrorMessage = "To date must be after from date")]
        public DateOnly? ContractStartDateTo { get; set; }

        public DateOnly? ContractEndDateFrom { get; set; }

        [DateRange("ContractEndDateFrom", ErrorMessage = "To date must be after from date")]
        public DateOnly? ContractEndDateTo { get; set; }
    }
}