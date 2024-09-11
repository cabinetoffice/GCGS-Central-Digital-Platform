using Flurl;
using Flurl.Http;
using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients;

public class CompaniesHouseApi(IConfiguration configuration) : ICompaniesHouseApi
{
    public async Task<RegisteredAddress> GetRegisteredAddress(
        string companyNumber)
    {
        var companiesHouseUrl = configuration["CompaniesHouse:Url"];
        var userName = configuration["CompaniesHouse:User"];
        var password = configuration["CompaniesHouse:Password"];

        var companyRegDetails = await $"{companiesHouseUrl}"
            .AppendPathSegment($"/company/{companyNumber}/registered-office-address")
            .WithBasicAuth(userName, password)
            .GetAsync()
            .ReceiveJson<RegisteredAddress>();

        return companyRegDetails;
    }

    public record RegisteredAddress
    {
        [JsonPropertyName("accept_appropriate_office_address_statement")] public bool AcceptAppropriateOfficeAddressStatement { get; init; }
        [JsonPropertyName("address_line_1")] public string? AddressLine1 { get; init; }
        [JsonPropertyName("address_line_2")] public string? AddressLine2 { get; init; }
        [JsonPropertyName("country")] public string? Country { get; init; }
        [JsonPropertyName("etag")] public string? Etag { get; init; }
        [JsonPropertyName("kind")] public string? Kind { get; init; }
        [JsonPropertyName("links")] public Links? Links { get; init; }
        [JsonPropertyName("locality")] public string? Locality { get; init; }
        [JsonPropertyName("postal_code")] public string? PostalCode { get; init; }
        [JsonPropertyName("premises")] public string? Premises { get; init; }
        [JsonPropertyName("region")] public string? Region { get; init; }
    }

    public record Links
    {
        [JsonPropertyName("self")]
        public Uri? Self { get; init; }
    }
}
