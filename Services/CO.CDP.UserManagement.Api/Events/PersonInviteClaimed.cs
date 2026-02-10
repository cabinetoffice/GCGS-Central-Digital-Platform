namespace CO.CDP.UserManagement.Api.Events;

public record PersonInviteClaimed
{
    public required Guid PersonInviteGuid { get; init; }
    public required Guid PersonGuid { get; init; }
    public required string UserUrn { get; init; }
    public required Guid OrganisationGuid { get; init; }
}
