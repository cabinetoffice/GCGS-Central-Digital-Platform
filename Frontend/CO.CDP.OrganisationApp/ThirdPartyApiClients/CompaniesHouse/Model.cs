using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients;

public record CompanyProfile
{
    [JsonPropertyName("company_name")] public string? CompanyName{ get; init; }
}

public record RegisteredAddress
{
    [JsonPropertyName("address_line_1")] public string? AddressLine1 { get; init; }
    [JsonPropertyName("address_line_2")] public string? AddressLine2 { get; init; }
    [JsonPropertyName("country")] public string? Country { get; init; }
    [JsonPropertyName("locality")] public string? Locality { get; init; }
    [JsonPropertyName("postal_code")] public string? PostalCode { get; init; }
}
