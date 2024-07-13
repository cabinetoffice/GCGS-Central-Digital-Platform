using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityState
{
    public Guid? SupplierOrganisationId { get; set; }
    public Guid? ConnectedEntityId { get; set; }
    public bool? SupplierHasCompanyHouseNumber { get; set; }
    public ConnectedEntityType? ConnectedEntityType { get; set; }

    public Address? RegisteredAddress { get; set; }
    public Address? PostalAddress { get; set; }

    public class Address
    {
        public string? AddressLine1 { get; set; }
        public string? TownOrCity { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
    }

    public void UpdateState(Guid supplierOrganisationId, Organisation.WebApiClient.ConnectedEntity connectedEntity)
    {
        SupplierOrganisationId = supplierOrganisationId;
        SupplierHasCompanyHouseNumber = connectedEntity.HasCompnayHouseNumber;
        ConnectedEntityId = connectedEntity.Id;
        ConnectedEntityType = connectedEntity.EntityType.AsConnectedEntityType();
    }
}