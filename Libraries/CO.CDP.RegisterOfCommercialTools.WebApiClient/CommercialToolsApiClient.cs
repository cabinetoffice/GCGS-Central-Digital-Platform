using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using System.Net.Http.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient;

public class CommercialToolsApiClient : ICommercialToolsApiClient
{
    private readonly HttpClient _httpClient;

    public CommercialToolsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SearchResponse?> SearchAsync(SearchRequestDto request)
    {
        try
        {
            var queryString = ToQueryString(request);
            return await _httpClient.GetFromJsonAsync<SearchResponse>($"api/Search?{queryString}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<CpvCodeDto>?> GetRootCpvCodesAsync(Culture culture = Culture.English)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CpvCodeDto>>($"api/CpvCode/root?culture={culture}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<CpvCodeDto>?> GetCpvChildrenAsync(string parentCode, Culture culture = Culture.English)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CpvCodeDto>>(
                $"api/CpvCode/{Uri.EscapeDataString(parentCode)}/children?culture={culture}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<CpvCodeDto>?> SearchCpvCodesAsync(string query, Culture culture = Culture.English)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CpvCodeDto>>(
                $"api/CpvCode/search?query={Uri.EscapeDataString(query)}&culture={culture}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<CpvCodeDto>?> GetCpvCodesAsync(List<string> codes)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/CpvCode/lookup", codes);
            return await response.Content.ReadFromJsonAsync<List<CpvCodeDto>>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<CpvCodeDto>?> GetCpvHierarchyAsync(string code)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CpvCodeDto>>(
                $"api/CpvCode/{Uri.EscapeDataString(code)}/hierarchy");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<NutsCodeDto>?> GetRootNutsCodesAsync(Culture culture = Culture.English)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NutsCodeDto>>($"api/NutsCode/root?culture={culture}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<NutsCodeDto>?> GetNutsChildrenAsync(string parentCode, Culture culture = Culture.English)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NutsCodeDto>>(
                $"api/NutsCode/{Uri.EscapeDataString(parentCode)}/children?culture={culture}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<NutsCodeDto>?> SearchNutsCodesAsync(string query, Culture culture = Culture.English)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NutsCodeDto>>(
                $"api/NutsCode/search?query={Uri.EscapeDataString(query)}&culture={culture}");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<NutsCodeDto>?> GetNutsCodesAsync(List<string> codes)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/NutsCode/lookup", codes);
            return await response.Content.ReadFromJsonAsync<List<NutsCodeDto>>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<NutsCodeDto>?> GetNutsHierarchyAsync(string code)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NutsCodeDto>>(
                $"api/NutsCode/{Uri.EscapeDataString(code)}/hierarchy");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static string ToQueryString(SearchRequestDto dto)
    {
        var properties = from p in dto.GetType().GetProperties()
            where p.GetValue(dto, null) != null
            select $"{p.Name}={Uri.EscapeDataString(p.GetValue(dto, null)?.ToString() ?? string.Empty)}";

        return string.Join("&", properties);
    }
}