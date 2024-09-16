using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;
public record ApprovableOrganisation
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required List<Identifier> Identifiers { get; init; } = [];

    public required string Role { get; init; }

    public required string? Email { get; init; }

    public string? Ppon { get; init; }

    public DateTimeOffset? ApprovedOn { get; init; }

    public Guid? ApprovedById { get; init; }

    public string? ApprovedByName { get; init; }

    public string? ApprovedComment{ get; init; }
}