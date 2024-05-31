using System.Net.Http.Headers;
using System.Text;

namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public class CompaniesHouseService : ICompaniesHouseService
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;

    public CompaniesHouseService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["CompaniesHouse:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "API Key is missing in configuration.");
        _baseUrl = configuration["CompaniesHouse:BaseUrl"] ?? throw new ArgumentNullException(nameof(configuration), "Base URL is missing in configuration.");
    }

    public async Task<CompanyHouseDetails> GetCompanyAsync(string companyNumber)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/company/{companyNumber}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new UTF8Encoding().GetBytes($"{_apiKey}:")));

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CompanyHouseDetails>() ?? new CompanyHouseDetails();
        }
        else
        {
            throw new Exception("Unable to fetch company details");
        }
    }
    public async Task<List<Officer>> GetCompanyOfficersListAsync(string companyNumber)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/company/{companyNumber}/officers");
        var byteArray = new UTF8Encoding().GetBytes($"{_apiKey}:");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var officerDetails = await response.Content.ReadFromJsonAsync<CompanyOfficerDetails>() ?? new CompanyOfficerDetails();
            return officerDetails.Officers?.Where(o => string.IsNullOrEmpty(o.ResignedOn)).ToList() ?? new List<Officer>();
        }
        else
        {
            throw new Exception("Unable to fetch company officers details");
        }
    }
}