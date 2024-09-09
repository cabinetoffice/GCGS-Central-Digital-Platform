using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Person.WebApi.Model;

public record RegisterPerson
{
    [Required(AllowEmptyStrings = true)] public required string FirstName { get; init; }
    [Required(AllowEmptyStrings = true)] public required string LastName { get; init; }
    [EmailAddress] public required string Email { get; init; }
    public string? Phone { get; init; }
    public string? UserUrn { get; init; }
}

internal record UpdatePerson
{
    [Required(AllowEmptyStrings = true)] public required string FirstName { get; init; }
    [Required(AllowEmptyStrings = true)] public required string LastName { get; init; }
    [EmailAddress] public required string Email { get; init; }
    public string? Phone { get; init; }
}

public record ClaimPersonInvite
{
    public required Guid PersonInviteId { get; init; }
}