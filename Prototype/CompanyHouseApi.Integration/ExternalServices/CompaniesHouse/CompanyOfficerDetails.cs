using System.Text.Json.Serialization;

namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;
public class CompanyOfficerDetails
{
    [JsonPropertyName("active_count")]
    public int ActiveCount { get; set; }

    [JsonPropertyName("etag")]
    public string? Etag { get; set; }

    [JsonPropertyName("items")]
    public List<Officer>? Officers { get; set; }

    [JsonPropertyName("resigned_count")]
    public int ResignedCount { get; set; }

    [JsonPropertyName("inactive_count")]
    public int InactiveCount { get; set; }

    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}

public class Officer
{
    [JsonPropertyName("address")]
    public Address? Address { get; set; }

    [JsonPropertyName("appointed_on")]
    public string? AppointedOn { get; set; }

    [JsonPropertyName("country_of_residence")]
    public string? CountryOfResidence { get; set; }

    [JsonPropertyName("date_of_birth")]
    public DateOfBirth? DateOfBirth { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }

    [JsonPropertyName("officer_role")]
    public string? OfficerRole { get; set; }

    [JsonPropertyName("person_number")]
    public string? PersonNumber { get; set; }

    [JsonPropertyName("resigned_on")]
    public string? ResignedOn { get; set; }
}

public class Address
{
    [JsonPropertyName("address_line_1")]
    public string? AddressLine1 { get; set; }

    [JsonPropertyName("address_line_2")]
    public string? AddressLine2 { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("locality")]
    public string? Locality { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("premises")]
    public string? Premises { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }
}

public class DateOfBirth
{
    [JsonPropertyName("month")]
    public int Month { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }
}