namespace CO.CDP.UserManagement.Api.Events;

public record PersonInviteClaimed
{
    public required string OrganisationId { get; init; }
    public required string PersonId { get; init; }
    public required string UserPrincipalId { get; init; }
    public required List<string> Scopes { get; init; }
}