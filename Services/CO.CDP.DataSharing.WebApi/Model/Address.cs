using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record Address
{
    [Required(AllowEmptyStrings = true)] public required string StreetAddress { get; init; }
    [Required(AllowEmptyStrings = true)] public required string Locality { get; init; }
    public string? Region { get; init; }
    public string? PostalCode { get; init; }
}