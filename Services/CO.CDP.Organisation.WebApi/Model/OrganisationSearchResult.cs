using CO.CDP.OrganisationInformation;
using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.WebApi.Model;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#parties">Party</a>.
/// </summary>
public record OrganisationSearchResult
{
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required Guid Id { get; init; }

    /// <example>"Acme Corporation"</example>
    public required string Name { get; init; }

    public required OrganisationType Type { get; init; }

    public required Identifier Identifier { get; init; }

    /// <example>["supplier"]</example>
    public required List<PartyRole> Roles { get; init; }
}