using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Models;

/// <summary>
/// Represents a child organisation with essential identification information.
/// This record is used to display child organisations in the parent-child relationship views.
/// </summary>
public record ChildOrganisation
{
    /// <summary>
    /// The name of the child organisation
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The unique identifier of the child organisation
    /// </summary>
    public Guid OrganisationId { get; init; }

    /// <summary>
    /// The organisation identifier (e.g. "DUNS", "PPON")
    /// </summary>
    public Identifier Identifier { get; init; }

    /// <summary>
    /// Creates a new instance of the ChildOrganisation record
    /// </summary>
    /// <param name="name">The name of the organisation</param>
    /// <param name="organisationId">The unique identifier of the organisation</param>
    /// <param name="identifier">The organisation identifier</param>
    public ChildOrganisation(string name, Guid organisationId, Identifier identifier)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        OrganisationId = organisationId;
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
    }

    /// <summary>
    /// Creates a formatted identifier string in the format "Scheme: Id"
    /// </summary>
    /// <returns>A formatted identifier string</returns>
    public string GetFormattedIdentifier() => $"{Identifier.Scheme}: {Identifier.Id}";

    public string GetIdentifierAsString() => $"{Identifier.Scheme}:{Identifier.Id}";
}