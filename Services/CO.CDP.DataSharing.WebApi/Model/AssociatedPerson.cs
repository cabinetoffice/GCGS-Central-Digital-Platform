using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.Model;

public record AssociatedPerson
{
    /// <example>"c16f9f7b-3f10-42db-86f8-93607b034a4c"</example>
    [Required] public required Guid Id { get; init; }
    /// <example>"Alice Doe"</example>
    [Required] public required string Name { get; init; }
    /// <example>"Company Director"</example>
    [Required] public required string Relationship { get; init; }

    /// <example>"https://cdp.cabinetoffice.gov.uk/persons/c16f9f7b-3f10-42db-86f8-93607b034a4c"</example>
    public Uri? Uri { get; init; }
    [Required] public required List<PartyRole> Roles { get; init; }

    public required AssociatedPersonDetails Details { get; set; }
}

public record AssociatedPersonDetails
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? ResidentCountry { get; set; }
    public IEnumerable<ControlCondition> ControlCondition { get; set; } = [];
    public ConnectedPersonType ConnectedType { get; set; }


    public IEnumerable<Address> Addresses { get; set; } = [];
    public DateTimeOffset? RegisteredDate { get; set; }
    public string? RegistrationAuthority { get; set; }
    public bool HasCompanyHouseNumber { get; set; }
    public string? CompanyHouseNumber { get; set; }
    public string? OverseasCompanyNumber { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}