using System.Text.Json.Serialization;

namespace CompanyHouseApi.Integration.ExternalServices.CompaniesHouse;

public class PersonsWithSignificantControl
{
    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("active_count")]
    public int ActiveCount { get; set; }

    [JsonPropertyName("items")]
    public List<PersonWithSignificantControl>? Persons { get; set; }

    [JsonPropertyName("items_per_page")]
    public int ItemsPerPage { get; set; }

    [JsonPropertyName("ceased_count")]
    public int CeasedCount { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}

public class PersonWithSignificantControl
{
    [JsonPropertyName("date_of_birth")]
    public DateOfBirth? DateOfBirth { get; set; }

    [JsonPropertyName("nationality")]
    public string? Nationality { get; set; }

    [JsonPropertyName("notified_on")]
    public string? NotifiedOn { get; set; }

    [JsonPropertyName("address")]
    public Address? Address { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("etag")]
    public string? Etag { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("country_of_residence")]
    public string? CountryOfResidence { get; set; }

    [JsonPropertyName("natures_of_control")]
    public List<string>? NaturesOfControl { get; set; }

    [JsonPropertyName("name_elements")]
    public NameElements? NameElements { get; set; }
}

public class NameElements
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    [JsonPropertyName("forename")]
    public string? Forename { get; set; }

    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }
}