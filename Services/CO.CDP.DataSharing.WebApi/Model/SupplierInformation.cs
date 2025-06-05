using CO.CDP.OrganisationInformation;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/reference/#parties">Parties</a>.
/// </summary>
public record SupplierInformation
{
    /// <example>"47e6a363-11c0-4cf4-bce6-dea03034e4bb"</example>
    [Required] public required Guid Id { get; init; }
    /// <example>"47e6a363-11c0-4cf4-bce6-dea03034e4bb"</example>
    [Required] public required OrganisationType Type { get; init; }
    /// <example>"Acme Corporation"</example>
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    [Required] public required List<AssociatedPerson> AssociatedPersons { get; init; } = [];
    [Required] public required List<AssociatedEntity> AdditionalEntities { get; init; } = [];
    [Required] public required List<OrganisationReference> AdditionalParties { get; set; } = [];
    [Required] public required OrganisationInformation.Identifier Identifier { get; init; }
    [Required] public required List<OrganisationInformation.Identifier> AdditionalIdentifiers { get; init; } = [];
    [Required] public required Address Address { get; init; }
    [Required] public required List<Address> AdditionalAddresses { get; init; } = [];
    [Required] public required ContactPoint ContactPoint { get; init; }

    /// <example>["supplier"]</example>
    [Required] public required List<PartyRole> Roles { get; init; } = [];
    [Required] public required Details Details { get; set; }
    [Required] public required SupplierInformationData SupplierInformationData { get; init; }
}