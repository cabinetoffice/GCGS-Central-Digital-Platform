using AutoMapper;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CommercialToolsService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<CommercialToolsService> logger,
    IMapper mapper) : ICommercialToolsService
{
    public async Task<(IEnumerable<SearchResultDto> results, int totalCount)> SearchCommercialToolsWithCount(
        string queryUrl)
    {
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", configuration.GetValue<string>("ODataApi:ApiKey"));
        logger.LogDebug("Calling ODI Commercial Tools API: {QueryUrl}", queryUrl);

        var response = await httpClient.GetAsync(queryUrl);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return ([], 0);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest &&
                errorContent.Contains("Could not find a property"))
            {
                logger.LogWarning("Query contains unsupported property. Returning empty results. Query: {QueryUrl}", queryUrl);
                return ([], 0);
            }

            logger.LogError("ODI Commercial Tools API error: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode();
        }

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<List<CommercialToolApiItem>>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var results = apiResponse?
            .Select(mapper.Map<SearchResultDto>)
            .ToList() ?? [];

        var totalCount =
            response.Headers.TryGetValues("x-total-count", out var totalCountValues)
            && int.TryParse(totalCountValues.FirstOrDefault(), out var parsedTotalCount)
                ? parsedTotalCount
                : 0;

        return (results, totalCount);
    }
}