using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Services;

public class CommercialToolsRepository : ICommercialToolsRepository
{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;

    public CommercialToolsRepository(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration.GetValue<string>("ODataApi:ApiKey"));
    }

    public async Task<IEnumerable<SearchResultDto>> SearchCommercialTools(string queryUrl)
    {
        var response = await _httpClient.GetAsync(queryUrl);
        
        // Handle 404 gracefully - return empty results instead of throwing
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Enumerable.Empty<SearchResultDto>();
        }
        
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawResults = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(content, options);

        var processedResults = new List<SearchResultDto>();
        foreach (var rawResult in rawResults ?? Enumerable.Empty<JsonElement>())
        {
            var dto = new SearchResultDto
            {
                Id = rawResult.TryGetProperty("releases", out var releasesElement) &&
                     releasesElement.ValueKind == JsonValueKind.Array &&
                     releasesElement.EnumerateArray().Any() &&
                     releasesElement.EnumerateArray().First().TryGetProperty("id", out var releaseIdElement)
                     ? releaseIdElement.GetString() ?? ""
                     : rawResult.GetProperty("id").GetString() ?? "",
                Title = rawResult.GetProperty("title").GetString() ?? "",
                Description = rawResult.GetProperty("description").GetString() ?? "",
                Link = rawResult.GetProperty("link").GetString() ?? "",
                PublishedDate = rawResult.GetProperty("publishedDate").GetDateTime(),
                SubmissionDeadline = rawResult.TryGetProperty("tenderPeriod", out var tenderPeriodElement) &&
                                     tenderPeriodElement.TryGetProperty("endDate", out var endDateElement)
                                     ? endDateElement.GetDateTime()
                                     : (DateTime?)null,
                Fees = rawResult.GetProperty("fees").GetDecimal(),
                AwardMethod = rawResult.GetProperty("awardMethod").GetString() ?? ""
            };

            var statusString = rawResult.GetProperty("status").GetString();
            if (dto.SubmissionDeadline.HasValue && dto.SubmissionDeadline.Value > DateTime.UtcNow && statusString != "Active" && statusString != "Awarded")
            {
                dto.Status = CommercialToolStatus.Upcoming;
            }
            else if (Enum.TryParse<CommercialToolStatus>(statusString, true, out var status))
            {
                dto.Status = status;
            }
            else
            {
                dto.Status = CommercialToolStatus.Unknown;
            }
            processedResults.Add(dto);
        }

        return processedResults;
    }

    public async Task<int> GetCommercialToolsCount(string queryUrl)
    {
        try
        {
            var countUrl = AddCountParameter(queryUrl);
            var response = await _httpClient.GetAsync(countUrl);
            
            // Handle 404 gracefully - return 0 count
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return 0;
            }
            
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            var result = JsonSerializer.Deserialize<JsonElement>(content, options);
            
            if (result.TryGetProperty("@odata.count", out var odataCount))
            {
                return odataCount.GetInt32();
            }
            
            if (result.TryGetProperty("totalCount", out var totalCountProperty))
            {
                return totalCountProperty.GetInt32();
            }
            
            if (result.TryGetProperty("meta", out var meta) && 
                meta.TryGetProperty("count", out var metaCount))
            {
                return metaCount.GetInt32();
            }
            
            var rawResults = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(content, options);
            return rawResults?.Count() ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string AddCountParameter(string queryUrl)
    {
        var separator = queryUrl.Contains("?") ? "&" : "?";
        return $"{queryUrl}{separator}$count=true";
    }

    public async Task<SearchResultDto?> GetCommercialToolById(string id)
    {
        var response = await _httpClient.GetAsync($"CommercialTools({id})");
        
        // Handle 404 gracefully - return null
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawResult = JsonSerializer.Deserialize<JsonElement>(content, options);

        var dto = new SearchResultDto
        {
            Id = rawResult.TryGetProperty("releases", out var releasesElement) &&
                 releasesElement.ValueKind == JsonValueKind.Array &&
                 releasesElement.EnumerateArray().Any() &&
                 releasesElement.EnumerateArray().First().TryGetProperty("id", out var releaseIdElement)
                 ? releaseIdElement.GetString() ?? ""
                 : rawResult.GetProperty("id").GetString() ?? "",
            Title = rawResult.GetProperty("title").GetString() ?? "",
            Description = rawResult.GetProperty("description").GetString() ?? "",
            Link = rawResult.GetProperty("link").GetString() ?? "",
            PublishedDate = rawResult.GetProperty("publishedDate").GetDateTime(),
            SubmissionDeadline = rawResult.TryGetProperty("tenderPeriod", out var tenderPeriodElement) &&
                                 tenderPeriodElement.TryGetProperty("endDate", out var endDateElement)
                                 ? endDateElement.GetDateTime()
                                 : (DateTime?)null,
            Fees = rawResult.GetProperty("fees").GetDecimal(),
            AwardMethod = rawResult.GetProperty("awardMethod").GetString() ?? "",
            ReservedParticipation = rawResult.TryGetProperty("tender", out var tenderElement) &&
                                    tenderElement.TryGetProperty("otherRequirements", out var otherRequirementsElement) &&
                                    otherRequirementsElement.TryGetProperty("reservedParticipation", out var reservedParticipationElement)
                                    ? reservedParticipationElement.GetString()
                                    : null
        };

        var statusString = rawResult.GetProperty("status").GetString();
        if (dto.SubmissionDeadline.HasValue && dto.SubmissionDeadline.Value > DateTime.UtcNow && statusString != "Active" && statusString != "Awarded")
        {
            dto.Status = CommercialToolStatus.Upcoming;
        }
        else if (Enum.TryParse<CommercialToolStatus>(statusString, true, out var status))
        {
            dto.Status = status;
        }
        else
        {
            dto.Status = CommercialToolStatus.Unknown;
        }

        return dto;
    }
}