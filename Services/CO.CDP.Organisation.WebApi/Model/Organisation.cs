using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record Organisation
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }

    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    [Required]
    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    [Required]
    public required OrganisationAddress Address { get; init; }

    public OrganisationContactPoint? ContactPoint { get; init; }

    public List<int>? Types { get; init; }
}

public record OrganisationAddress
{
    [Required(AllowEmptyStrings = false)]
    public required string AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string City { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string PostCode { get; init; }

    public string? Country { get; init; }
}

public record OrganisationContactPoint
{
    public string? Name { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string Email { get; init; }
    public string? Telephone { get; init; }
    public string? Url { get; init; }
}
