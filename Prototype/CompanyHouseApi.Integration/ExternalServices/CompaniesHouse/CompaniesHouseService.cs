using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public class CompaniesHouseService : ICompaniesHouseService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public CompaniesHouseService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["CompaniesHouse:ApiKey"];
    }

    public async Task<CompanyHouseDetails> GetCompanyAsync(string companyNumber)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/company/{companyNumber}");
        var byteArray = new UTF8Encoding().GetBytes($"{_apiKey}:");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CompanyHouseDetails>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        else
        {
            // Handle error response
            throw new Exception("Unable to fetch company details");
        }
    }
}