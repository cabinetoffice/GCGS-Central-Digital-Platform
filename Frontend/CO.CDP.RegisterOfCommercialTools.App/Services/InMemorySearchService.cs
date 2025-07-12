using CO.CDP.RegisterOfCommercialTools.App.Pages.Search;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class InMemorySearchService : ISearchService
{
    private readonly List<SearchResult> _allResults = new()
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

    public Task<(List<SearchResult> Results, int TotalCount)> SearchAsync(SearchModel searchModel, int pageNumber, int pageSize)
    {
        var results = _allResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((results, _allResults.Count));
    }
}

