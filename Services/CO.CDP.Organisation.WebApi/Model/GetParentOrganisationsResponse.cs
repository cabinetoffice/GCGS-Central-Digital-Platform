namespace CO.CDP.Organisation.WebApi.Model;

/// <summary>
/// Response model for retrieving parent organisations of a child organisation
/// </summary>
public class GetParentOrganisationsResponse
{
    /// <summary>
    /// The list of parent organisations with only the essential details needed by the frontend
    /// </summary>
    public IEnumerable<OrganisationSummary> ParentOrganisations { get; set; } = new List<OrganisationSummary>();

    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
}
