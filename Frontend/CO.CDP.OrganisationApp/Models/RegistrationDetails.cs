using CO.CDP.OrganisationApp.Constants;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Models;

public class RegistrationDetails
{
    public string? OrganisationName { get; set; }

    public string? OrganisationScheme { get; set; }

    public string? OrganisationEmailAddress { get; set; }

    public string? OrganisationIdentificationCountry { get; set; }

    public string? OrganisationIdentificationNumber { get; set; }

    public string? OrganisationAddressLine1 { get; set; }

    public string? OrganisationAddressLine2 { get; set; }

    public string? OrganisationCityOrTown { get; set; }

    public string? OrganisationRegion { get; set; }

    public string? OrganisationPostcode { get; set; }

    public string OrganisationCountryName { get; set; } = Country.UnitedKingdom;

    public string OrganisationCountryCode { get; set; } = Country.UKCountryCode;

    public OrganisationType? OrganisationType { get; set; }
    public bool? OrganisationHasCompaniesHouseNumber { get; set; }

    public string? BuyerOrganisationType { get; set; }
    public string? BuyerOrganisationOtherValue { get; set; }
    public bool? Devolved { get; set; }
    public List<DevolvedRegulation> Regulations { get; set; } = [];
    public List<OrganisationWebApiClient.OperationType> SupplierOrganisationOperationTypes { get; set; } = [];
}