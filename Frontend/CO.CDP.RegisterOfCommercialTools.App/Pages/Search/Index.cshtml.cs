using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages.Search
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public SearchModel SearchParams { get; set; } = new();

        public List<SearchResult> SearchResults { get; set; } = [];
        public PaginationPartialModel? Pagination { get; set; }

        [BindProperty(SupportsGet = true, Name = "pageNumber")]
        public int PageNumber { get; set; } = 1;

        private const int PageSize = 10;

        public void OnGet()
        {
            var allResults = new List<SearchResult>
            {
                new(
                    "Framework for Agile Delivery Services",
                    "Crown Commercial Service",
                    "/",
                    "Open framework scheme",
                    SearchResultStatus.Active,
                    "3%",
                    "Yes",
                    "10 February 2025",
                    "1 March 2025 to 28 February 2027",
                    "Without competition"
                ),
                new(
                    "Digital Outcomes and Specialists 6",
                    "Crown Commercial Service",
                    "/",
                    "Open framework scheme",
                    SearchResultStatus.Upcoming,
                    "1%",
                    "Yes",
                    "10 February 2025",
                    "1 March 2025 to 28 February 2027",
                    "Without competition"
                ),
                new(
                    "G-Cloud 13",
                    "Crown Commercial Service",
                    "/",
                    "Open framework scheme",
                    SearchResultStatus.Expired,
                    "0.75%",
                    "Yes",
                    "10 February 2023",
                    "1 March 2023 to 28 February 2024",
                    "Without competition"
                ),
                new(
                    "Vehicle Telematics/Hardware and Software Solutions",
                    "Crown Commercial Service",
                    "/",
                    "Dynamic purchasing system",
                    SearchResultStatus.Active,
                    "0.5%",
                    "Yes",
                    "10 February 2025",
                    "1 March 2025 to 28 February 2027",
                    "Without competition"
                ),
                new(
                    "Gigabit Capable Connectivity",
                    "Crown Commercial Service",
                    "/",
                    "Dynamic purchasing system",
                    SearchResultStatus.Active,
                    "1.5%",
                    "Yes",
                    "10 February 2025",
                    "1 March 2025 to 28 February 2027",
                    "Without competition"
                )
            };

            SearchResults = allResults.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();

            Pagination = new PaginationPartialModel
            {
                CurrentPage = PageNumber,
                PageSize = PageSize,
                TotalItems = allResults.Count,
                Url = "/search"
            };
        }
    }

    public record SearchResult(
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

    public class SearchModel
    {
        public string? Keywords { get; set; }
        public string? SortOrder { get; set; }

        [DisplayName("Framework options")]
        public string? FrameworkOptions { get; set; }

        [DisplayName("Dynamic market options")]
        public string? DynamicMarketOptions { get; set; }

        [DisplayName("Award method")]
        public bool AwardMethod { get; set; }

        public bool AwardMethodSet { get; set; }

        [DisplayName("Fee from")]
        [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
        public decimal? FeeFrom { get; set; }

        [DisplayName("Fee to")]
        [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
        public decimal? FeeTo { get; set; }

        [DisplayName("No fees")]
        public bool NoFees { get; set; }

        public bool NoFeesSet { get; set; }

        public List<string> Status { get; set; } = [];

        [DisplayName("Contracting authority usage")]
        public bool ContractingAuthorityUsage { get; set; }

        public bool ContractingAuthorityUsageSet { get; set; }

        [DisplayName("Submission deadline from")]
        public DateOnly? SubmissionDeadlineFrom { get; set; }

        [DisplayName("Submission deadline to")]
        public DateOnly? SubmissionDeadlineTo { get; set; }

        [DisplayName("Contract start date from")]
        public DateOnly? ContractStartDateFrom { get; set; }

        [DisplayName("Contract start date to")]
        public DateOnly? ContractStartDateTo { get; set; }

        [DisplayName("Contract end date from")]
        public DateOnly? ContractEndDateFrom { get; set; }

        [DisplayName("Contract end date to")]
        public DateOnly? ContractEndDateTo { get; set; }
    }
}