using AutoMapper;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CommercialToolsService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<CommercialToolsService> logger,
    IMapper mapper) : ICommercialToolsService
{
    public async Task<ApiResult<(IEnumerable<SearchResultDto> results, int totalCount)>> SearchCommercialToolsWithCount(
        string queryUrl)
    {
        try
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", configuration.GetValue<string>("ODataApi:ApiKey"));
            logger.LogDebug("Calling ODI Commercial Tools API: {QueryUrl}", queryUrl);

            var response = await httpClient.GetAsync(queryUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogDebug("ODI Commercial Tools API returned 404 - no results found for query: {QueryUrl}", queryUrl);
                return ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((Enumerable.Empty<SearchResultDto>(), 0));
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var logLevel = (int)response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
                logger.Log(logLevel, "ODI Commercial Tools API returned {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

                return (int)response.StatusCode >= 500
                    ? ApiResult<(IEnumerable<SearchResultDto>, int)>.Failure(new ServerError($"ODI API error: {response.StatusCode}", response.StatusCode))
                    : ApiResult<(IEnumerable<SearchResultDto>, int)>.Failure(new ClientError($"ODI API error: {response.StatusCode}", response.StatusCode));
            }

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<List<CommercialToolApiItem>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse == null)
            {
                logger.LogWarning("ODI API returned null response for query: {QueryUrl}", queryUrl);
                return ApiResult<(IEnumerable<SearchResultDto>, int)>.Failure(new DeserialisationError("ODI API returned null response"));
            }

            var results = apiResponse
                .DistinctBy(item => item.Ocid) // temporary de-dupe until DAPS issue fixed
                .Select(mapper.Map<SearchResultDto>)
                .ToList();

            var totalCount =
                response.Headers.TryGetValues("x-total-count", out var totalCountValues)
                && int.TryParse(totalCountValues.FirstOrDefault(), out var parsedTotalCount)
                    ? parsedTotalCount
                    : 0;

            return ApiResult<(IEnumerable<SearchResultDto>, int)>.Success((results, totalCount));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling ODI Commercial Tools API: {QueryUrl}", queryUrl);
            return ApiResult<(IEnumerable<SearchResultDto>, int)>.Failure(new ServerError("Error calling ODI API", System.Net.HttpStatusCode.InternalServerError));
        }
    }
}