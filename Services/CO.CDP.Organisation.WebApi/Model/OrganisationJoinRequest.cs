using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;

public record OrganisationJoinRequest
{
    public required Guid Id { get; init; }
    public required Person Person { get; init; }
    public required Organisation Organisation { get; init; }
    public Person? ReviewedBy { get; init; }
    public DateTimeOffset? ReviewedOn { get; set; }
    public required OrganisationJoinRequestStatus Status { get; set; }
}