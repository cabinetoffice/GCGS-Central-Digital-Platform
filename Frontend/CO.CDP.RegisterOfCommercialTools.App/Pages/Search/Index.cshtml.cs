using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages.Search
{
    public class IndexModel : PageModel
    {
        public List<SearchResult> SearchResults { get; set; } = [];

        public void OnGet()
        {
            SearchResults =
            [
                new SearchResult(
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
                new SearchResult(
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
                new SearchResult(
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
                )
            ];
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
}