using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Models;

public class OrganisationDetails
{
    public Guid? OrganisationId { get; set; }

    public SupplierType? SupplierType { get; set; }
}