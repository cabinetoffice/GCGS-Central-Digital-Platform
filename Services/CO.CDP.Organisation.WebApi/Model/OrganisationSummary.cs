using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

/// <summary>
/// Simplified organisation model
/// </summary>
public class OrganisationSummary
{
    /// <summary>
    /// The unique identifier of the organisation
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the organisation
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The roles of the child organisation
    /// </summary>
    public IList<PartyRole> Roles { get; set; } = new List<PartyRole>();

    /// <summary>
    /// The PPON of the child organisation, returned without the prefix
    /// </summary>
    public string Ppon { get; set; } = string.Empty;
}
