using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using System.Security.Cryptography;
using System.Text;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services.Caching;

public static class CacheKeyBuilder
{
    private const string SearchPrefix = "search";

    public static string BuildSearchKey(SearchRequestDto request) =>
        string.Join(":", GetKeyComponents(request).Where(c => !string.IsNullOrEmpty(c)));

    private static IEnumerable<string> GetKeyComponents(SearchRequestDto request) =>
    [
        SearchPrefix,
        FormatKeywords(request.Keywords),
        $"mode:{request.SearchMode}",
        FormatStatus(request.Status),
        $"page:{request.PageNumber}",
        FormatOptional("skip", request.Skip),
        FormatOptional("top", request.Top),
        FormatOptional("sortBy", request.SortBy),
        FormatCodes("cpv", request.CpvCodes),
        FormatCodes("loc", request.LocationCodes),
        FormatOptional("minFees", request.MinFees),
        FormatOptional("maxFees", request.MaxFees),
        FormatOptional("subFrom", request.SubmissionDeadlineFrom),
        FormatOptional("subTo", request.SubmissionDeadlineTo),
        FormatOptional("startDate", request.ContractStartDate),
        FormatOptional("endDate", request.ContractEndDate),
        FormatBoolean("frameworks", request.FilterFrameworks),
        FormatBoolean("openFrameworks", request.IsOpenFrameworks),
        FormatBoolean("dynamicMarkets", request.FilterDynamicMarkets),
        FormatBoolean("utilities", request.IsUtilitiesOnly),
        FormatOptional("frameworkOpts", request.FrameworkOptions),
        FormatOptional("dynamicOpts", request.DynamicMarketOptions),
        FormatCodes("caUsage", request.ContractingAuthorityUsage),
        FormatCodes("awardMethod", request.AwardMethod)
    ];

    private static string FormatKeywords(List<string>? keywords) =>
        keywords == null || keywords.Count == 0
            ? "kw:none"
            : $"kw:{HashList(keywords)}";

    private static string FormatStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "status:all" : $"status:{Hash(status)}";

    private static string FormatCodes(string prefix, List<string>? codes) =>
        codes == null || codes.Count == 0
            ? string.Empty
            : $"{prefix}:{HashList(codes)}";

    private static string FormatOptional<T>(string prefix, T? value) where T : struct =>
        value.HasValue ? $"{prefix}:{value}" : string.Empty;

    private static string FormatOptional(string prefix, string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : $"{prefix}:{value}";

    private static string FormatBoolean(string prefix, bool value) =>
        value ? $"{prefix}:true" : string.Empty;

    private static string HashList(List<string> items) =>
        Hash(string.Join(",", items.OrderBy(x => x)));

    private static string Hash(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..8];
    }
}