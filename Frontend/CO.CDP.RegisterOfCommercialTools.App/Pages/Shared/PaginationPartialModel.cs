using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;

public class PaginationPartialModel
{
    [Required]
    public required int CurrentPage { get; set; }

    [Required]
    public required int TotalItems { get; set; }

    [Required]
    public required int PageSize { get; set; }

    [Required]
    public required string Url { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public Func<int, string> GetPageUrl => (page) =>
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["pageNumber"] = page.ToString(),
            ["pageSize"] = PageSize.ToString()
        };

        return QueryHelpers.AddQueryString(Url, queryParams);
    };
}
