using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public class CompaniesHouseService : ICompaniesHouseService
{
    private readonly string _apiKey;
    private readonly HttpClient httpClient;

    public CompaniesHouseService(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        _apiKey = configuration["CompaniesHouse:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "API Key is missing in configuration.");
    }

    public async Task<CompanyHouseDetails> GetCompanyAsync(string companyNumber)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.company-information.service.gov.uk/company/{companyNumber}");
        var byteArray = new UTF8Encoding().GetBytes($"{_apiKey}:");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CompanyHouseDetails>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new CompanyHouseDetails();
        }
        else
        {
            // Handle error response
            throw new Exception("Unable to fetch company details");
        }
    }
}