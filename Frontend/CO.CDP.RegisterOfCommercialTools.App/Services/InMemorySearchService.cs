using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages;
using SearchResultStatus = CO.CDP.RegisterOfCommercialTools.App.Models.SearchResultStatus;

namespace CO.CDP.RegisterOfCommercialTools.App.Services;

public class InMemorySearchService : ISearchService
{
    private readonly List<SearchResult> _allResults =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Framework for Agile Delivery Services",
            "Crown Commercial Service",
            "Open framework scheme",
            SearchResultStatus.Active,
            "3%",
            "Yes",
            "10 February 2025",
            "1 March 2025 to 28 February 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Digital Outcomes and Specialists 6",
            "Crown Commercial Service",
            "Open framework scheme",
            SearchResultStatus.Upcoming,
            "1%",
            "Yes",
            "10 February 2025",
            "1 March 2025 to 28 February 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "G-Cloud 13",
            "Crown Commercial Service",
            "Open framework scheme",
            SearchResultStatus.Expired,
            "0.75%",
            "Yes",
            "10 February 2023",
            "1 March 2023 to 28 February 2024",
            "Without competition"
        ),
        new(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Vehicle Telematics/Hardware and Software Solutions",
            "Crown Commercial Service",
            "Dynamic purchasing system",
            SearchResultStatus.Active,
            "0.5%",
            "Yes",
            "10 February 2025",
            "1 March 2025 to 28 February 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Gigabit Capable Connectivity",
            "Crown Commercial Service",
            "Dynamic purchasing system",
            SearchResultStatus.Active,
            "1.5%",
            "Yes",
            "10 February 2025",
            "1 March 2025 to 28 February 2027",
            "Without competition"
        )
    ];

    private static int CalculateRelevanceScore(SearchResult result, string keywords)
    {
        if (string.IsNullOrWhiteSpace(keywords)) return 0;
        var title = result.Title;
        var tool = result.CommercialTool;
        var content = string.Join(" ", title, tool).ToLowerInvariant();
        var input = keywords.ToLowerInvariant().Trim();

        if (input.StartsWith("\"") && input.EndsWith("\""))
        {
            var phrase = input.Trim('"');
            return content.Contains(phrase) ? 1000 : 0;
        }

        var terms = input.Contains('+')
            ? input.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var allPresent = terms.All(t => content.Contains(t));
        var anyPresent = terms.Any(t => content.Contains(t));
        var frequency = terms.Sum(term =>
        {
            int count = 0, idx = 0;
            while ((idx = content.IndexOf(term, idx, StringComparison.Ordinal)) != -1)
            {
                count++;
                idx += term.Length;
            }
            return count;
        });

        var score = 0;
        if (allPresent) score += 500;
        if (anyPresent) score += 100;
        score += frequency * 10;
        return score;
    }

    public Task<(List<SearchResult> Results, int TotalCount)> SearchAsync(SearchModel searchModel, int pageNumber, int pageSize)
    {
        var query = _allResults.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchModel.Keywords))
        {
            var keywords = searchModel.Keywords.Trim();
            if (keywords.StartsWith("\"") && keywords.EndsWith("\""))
            {
                var phrase = keywords.Trim('"');
                query = query.Where(r =>
                    r.Title.Contains(phrase, StringComparison.OrdinalIgnoreCase) ||
                    r.CommercialTool.Contains(phrase, StringComparison.OrdinalIgnoreCase));
            }
            else if (keywords.Contains('+'))
            {
                var terms = keywords.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                query = query.Where(r => terms.All(term =>
                    r.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.CommercialTool.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                var terms = keywords.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                query = query.Where(r => terms.Any(term =>
                    r.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.CommercialTool.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }
        }

        var filteredResults = query.ToList();

        if (!string.IsNullOrEmpty(searchModel.SortOrder))
        {
            filteredResults = searchModel.SortOrder switch
            {
                "a-z" => filteredResults.OrderBy(r => r.Title).ToList(),
                "z-a" => filteredResults.OrderByDescending(r => r.Title).ToList(),
                "relevance" => filteredResults.OrderByDescending(r => CalculateRelevanceScore(r, searchModel.Keywords ?? "")).ToList(),
                _ => filteredResults
            };
        }
        else
        {
            filteredResults = filteredResults.OrderByDescending(r => CalculateRelevanceScore(r, searchModel.Keywords ?? "")).ToList();
        }

        var results = filteredResults.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult((results, filteredResults.Count));
    }

    public Task<SearchResult?> GetByIdAsync(Guid id)
    {
        var result = _allResults.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(result);
    }
}
