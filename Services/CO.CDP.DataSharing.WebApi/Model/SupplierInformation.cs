using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.Model;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#parties">Parties</a>.
/// </summary>
internal record SupplierInformation
{
    /// <example>"47e6a363-11c0-4cf4-bce6-dea03034e4bb"</example>
    [Required] public required Guid Id { get; init; }
    /// <example>"Acme Corporation"</example>
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    [Required] public required List<AssociatedPerson> AssociatedPersons { get; init; }
    [Required] public required List<OrganisationReference> AdditionalParties { get; init; }
    [Required] public required List<OrganisationReference> AdditionalEntities { get; init; }
    [Required] public required Identifier Identifier { get; init; }
    [Required] public required List<Identifier> AdditionalIdentifiers { get; init; }
    [Required] public required Address Address { get; init; }
    [Required] public required ContactPoint ContactPoint { get; init; }
    /// <example>["supplier"]</example>
    [Required] public required List<PartyRole> Roles { get; init; }
    [Required] public required Details Details { get; init; }
    [Required] public required SupplierInformationData SupplierInformationData { get; init; }
}