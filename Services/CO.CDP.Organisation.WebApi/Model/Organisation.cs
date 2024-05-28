using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record Organisation
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Identifier Identifier { get; init; }

    public List<Identifier> AdditionalIdentifiers { get; init; } = [];

    public required Address Address { get; init; }

    public ContactPoint? ContactPoint { get; init; }

    public List<int>? Types { get; init; }
}
