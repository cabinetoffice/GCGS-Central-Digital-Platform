using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record AssociatedPerson
{
    /// <example>"c16f9f7b-3f10-42db-86f8-93607b034a4c"</example>
    [Required] public required Guid Id { get; init; }
    /// <example>"Alice Doe"</example>
    [Required] public required string Name { get; init; }
    /// <example>"Company Director"</example>
    [Required] public required string Relationship { get; init; }
    /// <example>"https://cdp.cabinetoffice.gov.uk/persons/c16f9f7b-3f10-42db-86f8-93607b034a4c"</example>
    [Required] public required Uri Uri { get; init; }
    /// <example>4</example>
    [Required] public required int Role { get; init; }
}