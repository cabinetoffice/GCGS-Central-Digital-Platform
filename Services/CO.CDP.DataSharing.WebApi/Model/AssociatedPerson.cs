using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.Model;

public record AssociatedPerson
{
    /// <example>"c16f9f7b-3f10-42db-86f8-93607b034a4c"</example>
    [Required] public required Guid Id { get; init; }
    /// <example>"Alice Doe"</example>
    [Required] public required string Name { get; init; }
    /// <example>"Company Director"</example>
    [Required] public required string Relationship { get; init; }
    /// <example>"https://cdp.cabinetoffice.gov.uk/persons/c16f9f7b-3f10-42db-86f8-93607b034a4c"</example>
    [Required] public required Uri Uri { get; init; }
    [Required] public required List<PartyRole> Roles { get; init; }
}