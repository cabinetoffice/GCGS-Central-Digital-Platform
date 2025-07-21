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

    [DateComponentValidation("SubmissionDeadlineFrom")]
    public string? SubmissionDeadlineFromDay { get; set; }
    [DateComponentValidation("SubmissionDeadlineFrom")]
    public string? SubmissionDeadlineFromMonth { get; set; }
    [DateComponentValidation("SubmissionDeadlineFrom")]
    public string? SubmissionDeadlineFromYear { get; set; }

    [DateComponentValidation("SubmissionDeadlineTo")]
    public string? SubmissionDeadlineToDay { get; set; }
    [DateComponentValidation("SubmissionDeadlineTo")]
    public string? SubmissionDeadlineToMonth { get; set; }
    [DateComponentValidation("SubmissionDeadlineTo")]
    public string? SubmissionDeadlineToYear { get; set; }

    [DateComponentValidation("ContractStartDateFrom")]
    public string? ContractStartDateFromDay { get; set; }
    [DateComponentValidation("ContractStartDateFrom")]
    public string? ContractStartDateFromMonth { get; set; }
    [DateComponentValidation("ContractStartDateFrom")]
    public string? ContractStartDateFromYear { get; set; }

    [DateComponentValidation("ContractStartDateTo")]
    public string? ContractStartDateToDay { get; set; }
    [DateComponentValidation("ContractStartDateTo")]
    public string? ContractStartDateToMonth { get; set; }
    [DateComponentValidation("ContractStartDateTo")]
    public string? ContractStartDateToYear { get; set; }

    [DateComponentValidation("ContractEndDateFrom")]
    public string? ContractEndDateFromDay { get; set; }
    [DateComponentValidation("ContractEndDateFrom")]
    public string? ContractEndDateFromMonth { get; set; }
    [DateComponentValidation("ContractEndDateFrom")]
    public string? ContractEndDateFromYear { get; set; }

    [DateComponentValidation("ContractEndDateTo")]
    public string? ContractEndDateToDay { get; set; }
    [DateComponentValidation("ContractEndDateTo")]
    public string? ContractEndDateToMonth { get; set; }
    [DateComponentValidation("ContractEndDateTo")]
    public string? ContractEndDateToYear { get; set; }

    public DateOnly? SubmissionDeadlineFrom =>
        TryParseDate(SubmissionDeadlineFromDay, SubmissionDeadlineFromMonth, SubmissionDeadlineFromYear);

    public DateOnly? SubmissionDeadlineTo =>
        TryParseDate(SubmissionDeadlineToDay, SubmissionDeadlineToMonth, SubmissionDeadlineToYear);

    public DateOnly? ContractStartDateFrom =>
        TryParseDate(ContractStartDateFromDay, ContractStartDateFromMonth, ContractStartDateFromYear);

    public DateOnly? ContractStartDateTo =>
        TryParseDate(ContractStartDateToDay, ContractStartDateToMonth, ContractStartDateToYear);

    public DateOnly? ContractEndDateFrom =>
        TryParseDate(ContractEndDateFromDay, ContractEndDateFromMonth, ContractEndDateFromYear);

    public DateOnly? ContractEndDateTo =>
        TryParseDate(ContractEndDateToDay, ContractEndDateToMonth, ContractEndDateToYear);

    private static DateOnly? TryParseDate(string? day, string? month, string? year)
    {
        if (string.IsNullOrWhiteSpace(day) || string.IsNullOrWhiteSpace(month) || string.IsNullOrWhiteSpace(year))
            return null;

        if (!int.TryParse(day, out var d) || !int.TryParse(month, out var m) ||
            !int.TryParse(year, out var y)) return null;
        if (DateTime.TryParse($"{y}-{m:D2}-{d:D2}", out var date))
            return DateOnly.FromDateTime(date);

        return null;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(NoFees))
        {
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

        if (SubmissionDeadlineFrom.HasValue && SubmissionDeadlineTo.HasValue && SubmissionDeadlineTo < SubmissionDeadlineFrom)
        {
            yield return new ValidationResult(
                "Submission deadline to date must be after from date",
                [nameof(SubmissionDeadlineToDay), nameof(SubmissionDeadlineToMonth), nameof(SubmissionDeadlineToYear)]
            );
        }

        if (ContractStartDateFrom.HasValue && ContractStartDateTo.HasValue && ContractStartDateTo < ContractStartDateFrom)
        {
            yield return new ValidationResult(
                "Contract start date to date must be after from date",
                [nameof(ContractStartDateToDay), nameof(ContractStartDateToMonth), nameof(ContractStartDateToYear)]
            );
        }

        if (ContractEndDateFrom.HasValue && ContractEndDateTo.HasValue && ContractEndDateTo < ContractEndDateFrom)
        {
            yield return new ValidationResult(
                "Contract end date to date must be after from date",
                [nameof(ContractEndDateToDay), nameof(ContractEndDateToMonth), nameof(ContractEndDateToYear)]
            );
        }
    }
}