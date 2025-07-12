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
        ),
        new(
            Guid.Parse("66666666-6666-6666-6666-666666666666"),
            "Cloud Hosting and Support",
            "Department for Education",
            "Cloud services",
            SearchResultStatus.Active,
            "2%",
            "Yes",
            "15 March 2025",
            "1 April 2025 to 31 March 2027",
            "Direct Award"
        ),
        new(
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            "Market Research Services",
            "Home Office",
            "Research services",
            SearchResultStatus.Active,
            "1.2%",
            "No",
            "20 April 2025",
            "1 May 2025 to 30 April 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            "IT Hardware Procurement",
            "Ministry of Defence",
            "Procurement services",
            SearchResultStatus.Upcoming,
            "0.8%",
            "Yes",
            "10 May 2025",
            "1 June 2025 to 31 May 2027",
            "Direct Award"
        ),
        new(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "Digital Workplace Solutions",
            "Department of Health and Social Care",
            "Workplace solutions",
            SearchResultStatus.Active,
            "1.1%",
            "No",
            "5 June 2025",
            "1 July 2025 to 30 June 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Cyber Security Services",
            "Department for Transport",
            "Security services",
            SearchResultStatus.Active,
            "1.3%",
            "Yes",
            "12 July 2025",
            "1 August 2025 to 31 July 2027",
            "Direct Award"
        ),
        new(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "Facilities Management",
            "Ministry of Justice",
            "Facilities services",
            SearchResultStatus.Expired,
            "0.9%",
            "No",
            "18 August 2025",
            "1 September 2025 to 31 August 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            "Legal Advisory Services",
            "Department for Business and Trade",
            "Legal services",
            SearchResultStatus.Active,
            "1.4%",
            "Yes",
            "25 September 2025",
            "1 October 2025 to 30 September 2027",
            "Direct Award"
        ),
        new(
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            "Translation and Interpretation",
            "Foreign, Commonwealth & Development Office",
            "Language services",
            SearchResultStatus.Upcoming,
            "1.6%",
            "No",
            "2 October 2025",
            "1 November 2025 to 31 October 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            "Recruitment Services",
            "Department for Work and Pensions",
            "Recruitment",
            SearchResultStatus.Active,
            "1.7%",
            "Yes",
            "9 November 2025",
            "1 December 2025 to 30 November 2027",
            "Direct Award"
        ),
        new(
            Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            "Energy Supply Solutions",
            "Department for Environment, Food & Rural Affairs",
            "Energy services",
            SearchResultStatus.Active,
            "1.8%",
            "No",
            "16 December 2025",
            "1 January 2026 to 31 December 2027",
            "Without competition"
        ),
        new(
            Guid.Parse("10101010-1010-1010-1010-101010101010"),
            "Waste Management",
            "Department for Levelling Up, Housing & Communities",
            "Waste services",
            SearchResultStatus.Expired,
            "1.9%",
            "Yes",
            "23 January 2026",
            "1 February 2026 to 31 January 2028",
            "Direct Award"
        ),
        new(
            Guid.Parse("11111112-1111-1111-1111-111111111112"),
            "Office Supplies",
            "Department for Science, Innovation & Technology",
            "Office supplies",
            SearchResultStatus.Active,
            "2.0%",
            "No",
            "1 February 2026",
            "1 March 2026 to 28 February 2028",
            "Without competition"
        ),
        new(
            Guid.Parse("12121212-1212-1212-1212-121212121212"),
            "Travel Management",
            "Department for Culture, Media & Sport",
            "Travel services",
            SearchResultStatus.Active,
            "2.1%",
            "Yes",
            "8 March 2026",
            "1 April 2026 to 31 March 2028",
            "Direct Award"
        ),
        new(
            Guid.Parse("13131313-1313-1313-1313-131313131313"),
            "Insurance Brokerage",
            "Department for Energy Security & Net Zero",
            "Insurance services",
            SearchResultStatus.Upcoming,
            "2.2%",
            "No",
            "15 April 2026",
            "1 May 2026 to 30 April 2028",
            "Without competition"
        ),
        new(
            Guid.Parse("14141414-1414-1414-1414-141414141414"),
            "Catering Services",
            "Department for International Trade",
            "Catering",
            SearchResultStatus.Active,
            "2.3%",
            "Yes",
            "22 May 2026",
            "1 June 2026 to 31 May 2028",
            "Direct Award"
        ),
        new(
            Guid.Parse("15151515-1515-1515-1515-151515151515"),
            "Courier Services",
            "Department for Digital, Culture, Media & Sport",
            "Courier",
            SearchResultStatus.Active,
            "2.4%",
            "No",
            "29 June 2026",
            "1 July 2026 to 30 June 2028",
            "Without competition"
        ),
        new(
            Guid.Parse("16161616-1616-1616-1616-161616161616"),
            "Document Management",
            "Cabinet Office",
            "Document services",
            SearchResultStatus.Expired,
            "2.5%",
            "Yes",
            "6 July 2026",
            "1 August 2026 to 31 July 2028",
            "Direct Award"
        ),
        new(
            Guid.Parse("17171717-1717-1717-1717-171717171717"),
            "Fleet Management",
            "Department for Business, Energy & Industrial Strategy",
            "Fleet services",
            SearchResultStatus.Active,
            "2.6%",
            "No",
            "13 August 2026",
            "1 September 2026 to 31 August 2028",
            "Without competition"
        ),
        new(
            Guid.Parse("18181818-1818-1818-1818-181818181818"),
            "Event Management",
            "Department for International Development",
            "Event services",
            SearchResultStatus.Active,
            "2.7%",
            "Yes",
            "20 September 2026",
            "1 October 2026 to 30 September 2028",
            "Direct Award"
        ),
        new(
            Guid.Parse("19191919-1919-1919-1919-191919191919"),
            "Payroll Services",
            "Department for Exiting the European Union",
            "Payroll",
            SearchResultStatus.Upcoming,
            "2.8%",
            "No",
            "27 October 2026",
            "1 November 2026 to 31 October 2028",
            "Without competition"
        ),
        new(
            Guid.Parse("20202020-2020-2020-2020-202020202020"),
            "Learning and Development",
            "Department for Digital, Culture, Media & Sport",
            "Learning services",
            SearchResultStatus.Active,
            "2.9%",
            "Yes",
            "3 November 2026",
            "1 December 2026 to 30 November 2028",
            "Direct Award"
        ),
        new(
            Guid.Parse("21212121-2121-2121-2121-212121212121"),
            "Health and Wellbeing Services",
            "Department for Health and Social Care",
            "Health services",
            SearchResultStatus.Active,
            "3.0%",
            "No",
            "10 December 2026",
            "1 January 2027 to 31 December 2028",
            "Without competition"
        )
    ];

    public InMemorySearchService()
    {
        if (AppDomain.CurrentDomain.FriendlyName.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            _allResults = _allResults.Take(5).ToList();
        }
    }

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
