namespace CO.CDP.UserManagement.Api.Events;

public record OrganisationApproved
{
    public required string Id { get; init; }
}