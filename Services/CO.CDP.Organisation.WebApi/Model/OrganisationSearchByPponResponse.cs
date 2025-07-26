using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public class OrganisationSearchByPponResponse
{
    public IEnumerable<OrganisationSearchByPponResult> Results { get; set; } = new List<OrganisationSearchByPponResult>();
    public int TotalCount { get; set; }
}