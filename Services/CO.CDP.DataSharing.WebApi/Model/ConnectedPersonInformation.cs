using CO.CDP.OrganisationInformation;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.DataSharing.WebApi.Model;

public record ConnectedPersonInformation
(
    Guid PersonId,
    string FirstName,
    string LastName,
    string? Nationality,
    DateTimeOffset? DateOfBirth,
    ConnectedPersonType PersonType,
    ConnectedEntityIndividualAndTrustCategoryType Category,
    string? ResidentCountry,
    List<ConnectedAddress> Addresses,
    List<string> ControlConditions,
    string? CompanyHouseNumber,
    ConnectedIndividualTrust? IndividualTrust,
    ConnectedOrganisation? Organisation,
    OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityType EntityType
);

public record ConnectedIndividualTrust
(
    string FirstName,
    string LastName,
    DateTimeOffset? DateOfBirth,
    string? Nationality,
    ConnectedEntityIndividualAndTrustCategoryType Category,
    ConnectedPersonType PersonType,
    List<string> ControlConditions,
    string? ResidentCountry
);

public record ConnectedOrganisation
(
    string Name,
    string? RegisteredLegalForm,
    string? LawRegistered,
    List<string> ControlConditions,
    DateTimeOffset? InsolvencyDate,
    string? CompanyHouseNumber,
    string? OverseasCompanyNumber,
    Guid? OrganisationId
);

public record ConnectedAddress
(
    string StreetAddress,
    string Locality,
    string Region,
    string PostalCode,
    string CountryName,
    AddressType Type
);
