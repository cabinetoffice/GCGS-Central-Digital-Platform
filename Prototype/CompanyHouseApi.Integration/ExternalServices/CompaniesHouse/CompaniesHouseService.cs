using System.Net.Http.Headers;
using System.Text;

namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public class CompaniesHouseService : ICompaniesHouseService
{
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompaniesHouseService> _logger;

    public CompaniesHouseService(HttpClient httpClient, IConfiguration configuration, ILogger<CompaniesHouseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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

    public async Task<List<PersonWithSignificantControl>> GetPersonsWithSignificantControlAsync(string companyNumber)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/company/{companyNumber}/persons-with-significant-control");
        var byteArray = new UTF8Encoding().GetBytes($"{_apiKey}:");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var pscDetails = await response.Content.ReadFromJsonAsync<PersonsWithSignificantControl>() ?? new PersonsWithSignificantControl();
            return pscDetails.Persons?.ToList() ?? new List<PersonWithSignificantControl>();
        }
        else
        {
            throw new Exception("Unable to fetch persons with significant control details");
        }
    }
}