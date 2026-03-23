using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Person.WebApi.Model;

public record RegisterPerson
{
    [Required(AllowEmptyStrings = true)] public required string FirstName { get; init; }
    [Required(AllowEmptyStrings = true)] public required string LastName { get; init; }
    [EmailAddress] public required string Email { get; init; }
    public string? Phone { get; init; }
    public required string UserUrn { get; init; }
}

public record UpdatePerson
{
    public required string UserUrn { get; init; }
}

public record ClaimPersonInvite
{
    public required Guid PersonInviteId { get; init; }
}

public record LookupPerson
{
    public LookupPerson(string? urn, string? email)
    {
        Urn = urn;
        Email = email;
    }
    public string? Urn { get; }
    public string? Email { get; }
}

public record BulkLookupPerson
{
    [Required] public required IReadOnlyList<string> Urns { get; init; }
}
