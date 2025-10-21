using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;
using System.Net.Http.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient;

public class CommercialToolsApiClient : ICommercialToolsApiClient
{
    private readonly HttpClient _httpClient;

    public CommercialToolsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<SearchResponse>> SearchAsync(SearchRequestDto request)
    {
        return await ExecuteAsync(async () =>
        {
            var queryString = ToQueryString(request);
            return await _httpClient.GetAsync($"api/Search?{queryString}");
        }, async response => await response.Content.ReadFromJsonAsync<SearchResponse>());
    }

    public async Task<ApiResult<List<CpvCodeDto>>> GetRootCpvCodesAsync(Culture culture = Culture.English)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync($"api/CpvCode/root?culture={culture}"),
            async response => await response.Content.ReadFromJsonAsync<List<CpvCodeDto>>());
    }

    public async Task<ApiResult<List<CpvCodeDto>>> GetCpvChildrenAsync(string parentCode,
        Culture culture = Culture.English)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync(
                $"api/CpvCode/{Uri.EscapeDataString(parentCode)}/children?culture={culture}"),
            async response => await response.Content.ReadFromJsonAsync<List<CpvCodeDto>>());
    }

    public async Task<ApiResult<List<CpvCodeDto>>> SearchCpvCodesAsync(string query, Culture culture = Culture.English)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync(
                $"api/CpvCode/search?query={Uri.EscapeDataString(query)}&culture={culture}"),
            async response => await response.Content.ReadFromJsonAsync<List<CpvCodeDto>>());
    }

    public async Task<ApiResult<List<CpvCodeDto>>> GetCpvCodesAsync(List<string> codes)
    {
        return await ExecuteAsync(
            async () => await _httpClient.PostAsJsonAsync("api/CpvCode/lookup", codes),
            async response => await response.Content.ReadFromJsonAsync<List<CpvCodeDto>>());
    }

    public async Task<ApiResult<List<CpvCodeDto>>> GetCpvHierarchyAsync(string code)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync($"api/CpvCode/{Uri.EscapeDataString(code)}/hierarchy"),
            async response => await response.Content.ReadFromJsonAsync<List<CpvCodeDto>>());
    }

    public async Task<ApiResult<List<NutsCodeDto>>> GetRootNutsCodesAsync(Culture culture = Culture.English)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync($"api/NutsCode/root?culture={culture}"),
            async response => await response.Content.ReadFromJsonAsync<List<NutsCodeDto>>());
    }

    public async Task<ApiResult<List<NutsCodeDto>>> GetNutsChildrenAsync(string parentCode,
        Culture culture = Culture.English)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync(
                $"api/NutsCode/{Uri.EscapeDataString(parentCode)}/children?culture={culture}"),
            async response => await response.Content.ReadFromJsonAsync<List<NutsCodeDto>>());
    }

    public async Task<ApiResult<List<NutsCodeDto>>> SearchNutsCodesAsync(string query, Culture culture = Culture.English)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync(
                $"api/NutsCode/search?query={Uri.EscapeDataString(query)}&culture={culture}"),
            async response => await response.Content.ReadFromJsonAsync<List<NutsCodeDto>>());
    }

    public async Task<ApiResult<List<NutsCodeDto>>> GetNutsCodesAsync(List<string> codes)
    {
        return await ExecuteAsync(
            async () => await _httpClient.PostAsJsonAsync("api/NutsCode/lookup", codes),
            async response => await response.Content.ReadFromJsonAsync<List<NutsCodeDto>>());
    }

    public async Task<ApiResult<List<NutsCodeDto>>> GetNutsHierarchyAsync(string code)
    {
        return await ExecuteAsync(
            async () => await _httpClient.GetAsync($"api/NutsCode/{Uri.EscapeDataString(code)}/hierarchy"),
            async response => await response.Content.ReadFromJsonAsync<List<NutsCodeDto>>());
    }

    private async Task<ApiResult<T>> ExecuteAsync<T>(
        Func<Task<HttpResponseMessage>> httpCall,
        Func<HttpResponseMessage, Task<T?>> deserialize)
    {
        try
        {
            var response = await httpCall();

            if (response.IsSuccessStatusCode)
            {
                var data = await deserialize(response);
                if (data == null)
                {
                    return ApiResult<T>.Failure(new DeserialisationError("Failed to deserialise response content"));
                }

                return ApiResult<T>.Success(data);
            }

            if ((int)response.StatusCode >= 500)
            {
                return ApiResult<T>.Failure(new ServerError($"Server error: {response.StatusCode}", response.StatusCode));
            }

            return ApiResult<T>.Failure(new ClientError($"Client error: {response.StatusCode}", response.StatusCode));
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<T>.Failure(new NetworkError("Network error occurred", ex));
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure(new DeserialisationError("Unexpected error during API call", ex));
        }
    }

    private static string ToQueryString(SearchRequestDto dto)
    {
        var parameters = new List<string>();

        foreach (var property in dto.GetType().GetProperties())
        {
            var value = property.GetValue(dto, null);
            if (value == null) continue;

            if (value is System.Collections.IEnumerable enumerable and not string)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        parameters.Add($"{property.Name}={Uri.EscapeDataString(item.ToString() ?? string.Empty)}");
                    }
                }
            }
            else
            {
                var stringValue = value is DateTime dateTime
                    ? dateTime.ToString("yyyy-MM-dd")
                    : value.ToString() ?? string.Empty;
                parameters.Add($"{property.Name}={Uri.EscapeDataString(stringValue)}");
            }
        }

        return string.Join("&", parameters);
    }
}