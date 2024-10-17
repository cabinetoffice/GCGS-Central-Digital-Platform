using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record Details
{
    public Approval? Approval { get; init; }
    public List<PartyRole> PendingRoles { get; init; } = [];
}

public record Approval
{
    public DateTimeOffset? ApprovedOn { get; init; }
    public ReviewedBy? ReviewedBy { get; init; }
    public string? Comment { get; init; }
}

public record ReviewedBy
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
