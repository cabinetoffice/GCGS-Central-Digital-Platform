using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Models;

public class SupplierToBuyerDetails
{
    public OrganisationType? OrganisationType { get; set; }
    public string? BuyerOrganisationType { get; set; }
    public string? BuyerOrganisationOtherValue { get; set; }
    public bool? Devolved { get; set; }
    public List<DevolvedRegulation> Regulations { get; set; } = [];
}
