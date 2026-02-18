namespace CO.CDP.Person.WebApi.Events;

public record PersonInviteClaimed
{
    public required Guid PersonInviteGuid { get; init; }
    public required Guid PersonGuid { get; init; }
    public required string UserUrn { get; init; }
    public required Guid OrganisationGuid { get; init; }
}
