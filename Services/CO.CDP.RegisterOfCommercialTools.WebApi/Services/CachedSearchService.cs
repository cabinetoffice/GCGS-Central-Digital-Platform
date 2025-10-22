using CO.CDP.RegisterOfCommercialTools.WebApi.Helpers;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services.Caching;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CachedSearchService(
    ISearchService innerService,
    IRedisCacheService cacheService,
    IConfiguration configuration,
    ILogger<CachedSearchService> logger) : ISearchService
{
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(
        configuration.GetValue("Redis:DefaultExpirationMinutes", 60));

    public async Task<ApiResult<SearchResponse>> Search(SearchRequestDto request)
    {
        var cacheKey = CacheKeyBuilder.BuildSearchKey(request);

        logger.LogInformation("Searching with cache key: {CacheKey}", LogSanitizer.Sanitize(cacheKey));

        var cachedResponse = await cacheService.GetAsync<SearchResponse>(cacheKey);

        return await cachedResponse.Match(
            some: response =>
            {
                logger.LogInformation("Cache hit - returning {Count} cached results", response.Results.Count());
                return Task.FromResult(ApiResult<SearchResponse>.Success(response));
            },
            none: async () =>
            {
                logger.LogInformation("Cache miss - executing search query");
                var result = await innerService.Search(request);

                return result.Match(
                    error =>
                    {
                        logger.LogWarning("Search failed with error: {Error}", error.Message);
                        return ApiResult<SearchResponse>.Failure(error);
                    },
                    success =>
                    {
                        logger.LogInformation("Search completed with {Count} results, caching for {Expiration}",
                            success.Results.Count(), _cacheExpiration);
                        _ = cacheService.SetAsync(cacheKey, success, _cacheExpiration);
                        return ApiResult<SearchResponse>.Success(success);
                    });
            });
    }
}