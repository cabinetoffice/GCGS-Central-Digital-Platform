using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record SupplierInformation
{
    [Required] public required Guid Id { get; init; }
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    [Required] public required List<OrganisationReference> AdditionalParties { get; init; }
    [Required] public required Identifier Identifier { get; init; }
    [Required] public required List<Identifier> AdditionalIdentifiers { get; init; }
    [Required] public required Address Address { get; init; }
    [Required] public required List<ContactPoint> ContactPoints { get; init; }
    [Required] public required List<PartyRole> Roles { get; init; }
    [Required(AllowEmptyStrings = true)] public required string Details { get; init; }
    [Required] public required SupplierInformationData SupplierInformationData { get; init; }
}