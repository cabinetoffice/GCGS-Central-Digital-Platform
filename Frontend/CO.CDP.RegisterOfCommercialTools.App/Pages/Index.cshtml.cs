using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.App.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.UI.Foundation.Services;
using CO.CDP.UI.Foundation.Utilities;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages;

public class IndexModel(ISearchService searchService, ISirsiUrlService sirsiUrlService)
    : PageModel
{
    [BindProperty(SupportsGet = true)]
    public SearchModel SearchParams { get; set; } = new();

    public List<SearchResult> SearchResults { get; set; } = [];
    public PaginationPartialModel? Pagination { get; set; }

    [BindProperty(SupportsGet = true, Name = "pageNumber")]
    public int PageNumber { get; set; } = 1;

    private const int PageSize = 10;

    public string SirsiHomeUrl { get; private set; } = string.Empty;

    public int TotalCount { get; set; }

    public async Task OnGetAsync()
    {
        SirsiHomeUrl = sirsiUrlService.BuildUrl("/");
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
        CheckDateBindingErrors("SearchParams.SubmissionDeadlineFrom");
        CheckDateBindingErrors("SearchParams.SubmissionDeadlineTo");
        CheckDateBindingErrors("SearchParams.ContractStartDateFrom");
        CheckDateBindingErrors("SearchParams.ContractStartDateTo");
        CheckDateBindingErrors("SearchParams.ContractEndDateFrom");
        CheckDateBindingErrors("SearchParams.ContractEndDateTo");

        await OnGetAsync();
        return Page();
    }
    private void SanitiseSearchParams()
    {
        SearchParams.Keywords = InputSanitiser.SanitiseSingleLineTextInput(SearchParams.Keywords);
    }

    private void CheckDateBindingErrors(string key)
    {
        if (ModelState.TryGetValue(key, out var modelState) && modelState.Errors.Any(e => e.Exception is FormatException))
        {
            modelState.Errors.Clear();
            ModelState.AddModelError(key, "Please enter a valid date");
        }
    }

    public IActionResult OnPostReset()
    {
        return RedirectToPage();
    }

    public IActionResult OnPostGoToSirsi()
    {
        var sirsiHomeUrl = sirsiUrlService.BuildUrl("");
        return Redirect(sirsiHomeUrl);
    }
}


public class SearchModel : IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(NoFees))
        {
            yield break;
        }

        var feeFromHasValue = FeeFrom.HasValue;
        var feeToHasValue = FeeTo.HasValue;

        if (feeFromHasValue && feeToHasValue)
        {
            yield return new ValidationResult(
                "Fee from and to cannot be provided when 'No fees' is selected",
                [nameof(FeeFrom), nameof(FeeTo)]
            );
        }
        else if (feeFromHasValue)
        {
            yield return new ValidationResult(
                "Fee from cannot be provided when 'No fees' is selected",
                [nameof(FeeFrom)]
            );
        }
        else if (feeToHasValue)
        {
            yield return new ValidationResult(
                "Fee to cannot be provided when 'No fees' is selected",
                [nameof(FeeTo)]
            );
        }
    }
}