using CO.CDP.OrganisationApp.Constants;
using System;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityState
{
    public Guid? SupplierOrganisationId { get; set; }
    public Guid? ConnectedEntityId { get; set; }
    public bool? SupplierHasCompanyHouseNumber { get; set; }
    public ConnectedEntityType? ConnectedEntityType { get; set; }
    public ConnectedEntityOrganisationCategoryType? ConnectedEntityOrganisationCategoryType { get; set; }
    public string? OrganisationName { get; set; }
    public Address? RegisteredAddress { get; set; }
    public Address? PostalAddress { get; set; }

    public class Address
    {
        public string? AddressLine1 { get; set; }
        public string? TownOrCity { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
        public bool IsNonUk => Country != Constants.Country.UnitedKingdom;
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
            Constants.ConnectedEntityType.Individual => "",
            Constants.ConnectedEntityType.TrustOrTrustee => "",
            _ => "",
        };
    }
}