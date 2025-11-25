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
    public async Task<ApiResult<(IEnumerable<SearchResultDto> results, int totalCount, int filteredCount)>> SearchCommercialToolsWithCount(
        string queryUrl)
    {
        try
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", configuration.GetValue<string>("ODataApi:ApiKey"));
            logger.LogDebug("Calling ODI CommercialTools API: {QueryUrl}", queryUrl);

            var response = await httpClient.GetAsync(queryUrl, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogDebug("ODI CommercialTools API returned 404 - no results found for query: {QueryUrl}", queryUrl);
                return ApiResult<(IEnumerable<SearchResultDto>, int, int)>.Success((Enumerable.Empty<SearchResultDto>(), 0, 0));
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var logLevel = (int)response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
                logger.Log(logLevel, "ODI CommercialTools API returned {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

                return (int)response.StatusCode >= 500
                    ? ApiResult<(IEnumerable<SearchResultDto>, int, int)>.Failure(new ServerError($"ODI API error: {response.StatusCode}", response.StatusCode))
                    : ApiResult<(IEnumerable<SearchResultDto>, int, int)>.Failure(new ClientError($"ODI API error: {response.StatusCode}", response.StatusCode));
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            await using var stream = await response.Content.ReadAsStreamAsync();
            var results = new List<SearchResultDto>();
            var seenOcids = new HashSet<string>();

            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<CommercialToolApiItem>(stream, options))
            {
                if (item?.Ocid == null || !seenOcids.Add(item.Ocid))
                {
                    continue;
                }
                results.Add(mapper.Map<SearchResultDto>(item));
            }

            var totalCount =
                response.Headers.TryGetValues("x-total-count", out var totalCountValues)
                && int.TryParse(totalCountValues.FirstOrDefault(), out var parsedTotalCount)
                    ? parsedTotalCount
                    : 0;

            var filteredCount =
                response.Headers.TryGetValues("x-filtered-count", out var filteredCountValues)
                && int.TryParse(filteredCountValues.FirstOrDefault(), out var parsedFilteredCount)
                    ? parsedFilteredCount
                    : 0;

            return ApiResult<(IEnumerable<SearchResultDto>, int, int)>.Success((results, totalCount, filteredCount));
        }
        catch (JsonException jex)
        {
            logger.LogError(jex, "Failed to deserialise ODI API response for query: {QueryUrl}", queryUrl);
            return ApiResult<(IEnumerable<SearchResultDto>, int, int)>.Failure(new DeserialisationError("ODI API returned an invalid JSON response"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling ODI Commercial Tools API: {QueryUrl}", queryUrl);
            return ApiResult<(IEnumerable<SearchResultDto>, int, int)>.Failure(new ServerError("Error calling ODI API", System.Net.HttpStatusCode.InternalServerError));
        }
    }
}