using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record Organisation
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

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
