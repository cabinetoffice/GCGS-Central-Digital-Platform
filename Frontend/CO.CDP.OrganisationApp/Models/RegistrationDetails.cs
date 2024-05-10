namespace CO.CDP.OrganisationApp.Models;

public class RegistrationDetails
{
    public Guid TenantId { get; set; }
    public required string UserPrincipal { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }

    public string? OrganisationName { get; set; }

    public string? OrganisationScheme { get; set; }

    public string? OrganisationEmailAddress { get; set; }

    public string? OrganisationIdentificationNumber { get; set; }

    public string? OrganisationAddressLine1 { get; set; }

    public string? OrganisationAddressLine2 { get; set; }

    public string? OrganisationCityOrTown { get; set; }

    public string? OrganisationPostcode { get; set; }

    public string? OrganisationCountry { get; set; }

    public Guid? OrganisationId { get; set; }

    public Guid? PersonId { get; set; }
    public bool? OrganisationHasCompaniesHouseNumber { get; set; }
}