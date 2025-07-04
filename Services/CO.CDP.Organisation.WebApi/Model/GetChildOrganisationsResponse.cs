namespace CO.CDP.Organisation.WebApi.Model;

/// <summary>
/// Response model for retrieving child organisations of a parent organisation
/// </summary>
public class GetChildOrganisationsResponse
{
    /// <summary>
    /// The list of child organisations with only the essential details needed by the frontend
    /// </summary>
    public IEnumerable<OrganisationSummary> ChildOrganisations { get; set; } = new List<OrganisationSummary>();

    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
}
