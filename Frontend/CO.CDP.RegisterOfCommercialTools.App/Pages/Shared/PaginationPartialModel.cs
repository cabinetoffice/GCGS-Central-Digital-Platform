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

    public Func<int, string> GetPageUrl => page =>
    {
        if (!Url.Contains('?'))
        {
            return QueryHelpers.AddQueryString(Url, "pageNumber", page.ToString());
        }

        var parts = Url.Split('?', 2);
        var basePath = parts[0];
        var existingQuery = parts[1];

        var existingParams = new Dictionary<string, string?>();
        foreach (var kvp in QueryHelpers.ParseQuery(existingQuery))
        {
            existingParams[kvp.Key] = kvp.Value.ToString();
        }

        existingParams["pageNumber"] = page.ToString();

        return QueryHelpers.AddQueryString(basePath, existingParams);
    };
}
