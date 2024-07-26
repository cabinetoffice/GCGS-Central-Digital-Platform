using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityState
{
    public Guid? SupplierOrganisationId { get; set; }
    public Guid? ConnectedEntityId { get; set; }
    public bool? SupplierHasCompanyHouseNumber { get; set; }
    public ConnectedEntityType? ConnectedEntityType { get; set; }
    public ConnectedEntityOrganisationCategoryType? ConnectedEntityOrganisationCategoryType { get; set; }
    public ConnectedEntityIndividualAndTrustCategoryType? ConnectedEntityIndividualAndTrustCategoryType { get; set; }
    public string? OrganisationName { get; set; }
    public Address? RegisteredAddress { get; set; }
    public Address? PostalAddress { get; set; }
    public string? LegalForm {  get; set; }
    public string? LawRegistered { get; set; }
    public bool? HasCompaniesHouseNumber { get; set; }
    public string? CompaniesHouseNumber { get; set; }
    public List<ConnectedEntityControlCondition> ControlConditions { get; set; } = [];
    public DateTimeOffset? RegistrationDate { get; set; }
    public string? RegisterName { get; set; }
    public DateTimeOffset? InsolvencyDate { get; set; }
    public bool? HasOscCompaniesHouseNumber { get; set; }
    public string? OscCompaniesHouseNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Nationality { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? DirectorLocation { get; set; }
    public class Address
    {
        public string? AddressLine1 { get; set; }
        public string? TownOrCity { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
        public bool IsNonUk => Country != Constants.Country.UnitedKingdom;

        public bool? AreSameAddress(Address? address)
        {
            if (address == null) return null;
            return AddressLine1 == address.AddressLine1
                    && TownOrCity == address.TownOrCity
                    && Postcode == address.Postcode
                    && Country == address.Country;
        }
    }

    public void UpdateState(Guid supplierOrganisationId, Organisation.WebApiClient.ConnectedEntity connectedEntity)
    {
        SupplierOrganisationId = supplierOrganisationId;
        ConnectedEntityId = connectedEntity.Id;
        SupplierHasCompanyHouseNumber = connectedEntity.HasCompnayHouseNumber;
        ConnectedEntityType = connectedEntity.EntityType.AsConnectedEntityType();
    }

    public string GetCaption()
    {
        return ConnectedEntityType switch
        {
            Constants.ConnectedEntityType.Organisation => ConnectedEntityOrganisationCategoryType?.Catption(SupplierHasCompanyHouseNumber ?? false) ?? "",
            Constants.ConnectedEntityType.Individual => ConnectedEntityIndividualAndTrustCategoryType?.Catption(SupplierHasCompanyHouseNumber ?? false) ?? "",
            Constants.ConnectedEntityType.TrustOrTrustee => ConnectedEntityIndividualAndTrustCategoryType?.Catption(SupplierHasCompanyHouseNumber ?? false) ?? "",
            _ => "",
        };
    }
}