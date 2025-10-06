using AutoMapper;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CommercialToolsService : ICommercialToolsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CommercialToolsService> _logger;
    private readonly IMapper _mapper;

    public CommercialToolsService(HttpClient httpClient, IConfiguration configuration,
        ILogger<CommercialToolsService> logger, IMapper mapper)
    {
        _httpClient = httpClient;
        _logger = logger;
        _mapper = mapper;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", configuration.GetValue<string>("ODataApi:ApiKey"));
    }


    public async Task<(IEnumerable<SearchResultDto> results, int totalCount)> SearchCommercialToolsWithCount(
        string queryUrl)
    {
        var response = await _httpClient.GetAsync(queryUrl);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return ([], 0);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Commercial Tools API returned {response.StatusCode}: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var apiResponse = JsonSerializer.Deserialize<List<CommercialToolApiItem>>(content, options);

        if (apiResponse == null)
        {
            _logger.LogWarning("Failed to deserialise API response");
            return (Enumerable.Empty<SearchResultDto>(), 0);
        }

        var results = new List<SearchResultDto>();
        foreach (var item in apiResponse)
        {
            var searchResult = _mapper.Map<SearchResultDto>(item);
            results.Add(searchResult);
        }

        // Total count will be provided in future update
        var totalCount = 0;
        _logger.LogInformation("Processed {ProcessedCount} results, total count: {TotalCount}", results.Count, totalCount);
        return (results, totalCount);
    }

}