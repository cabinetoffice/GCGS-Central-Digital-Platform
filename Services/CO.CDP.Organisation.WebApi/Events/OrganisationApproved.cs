namespace CO.CDP.Organisation.WebApi.Events;

public record OrganisationApproved
{
    /// <example>"d2dab085-ec23-481c-b970-ee6b372f9f57"</example>
    public required string Id { get; init; }
}