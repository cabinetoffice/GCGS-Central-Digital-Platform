using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationApp.CharityCommission;

public record CharityDetails
{
    [JsonPropertyName("charity_name")] public string? Name { get; init; }
    [JsonPropertyName("address_line_one")] public string? AddressLine1 { get; init; }
    [JsonPropertyName("address_line_two")] public string? AddressLine2 { get; init; }
    [JsonPropertyName("address_line_three")] public string? AddressLine3 { get; init; }
    [JsonPropertyName("address_line_four")] public string? AddressLine4 { get; init; }
    [JsonPropertyName("address_line_five")] public string? AddressLine5 { get; init; }
    [JsonPropertyName("address_post_code")] public string? PostalCode { get; init; }
    [JsonPropertyName("email")] public string? Email { get; init; }

}
