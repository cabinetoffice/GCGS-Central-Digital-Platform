using System.Text.Json.Serialization;

namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public class CompanyHouseDetails
{
    [JsonPropertyName("company_number")]
    public string CompanyNumber { get; set; }

    [JsonPropertyName("date_of_creation")]
    public string DateOfCreation { get; set; }

    [JsonPropertyName("last_full_members_list_date")]
    public string LastFullMembersListDate { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("jurisdiction")]
    public string Jurisdiction { get; set; }

    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; }

    [JsonPropertyName("registered_office_address")]
    public RegisteredOfficeAddress RegisteredOfficeAddress { get; set; }

    [JsonPropertyName("sic_codes")]
    public List<string> SicCodes { get; set; }

    [JsonPropertyName("company_status")]
    public string CompanyStatus { get; set; }
}

public class RegisteredOfficeAddress
{
    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    [JsonPropertyName("address_line_2")]
    public string AddressLine2 { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("address_line_1")]
    public string AddressLine1 { get; set; }

    [JsonPropertyName("locality")]
    public string Locality { get; set; }
}