using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Person.WebApi.Model;

public record RegisterPerson
{
    [Required(AllowEmptyStrings = true)] public required string FirstName { get; init; }
    [Required(AllowEmptyStrings = true)] public required string LastName { get; init; }
    public int? Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}

internal record UpdatePerson
{
    [Required(AllowEmptyStrings = true)] public required string FirstName { get; init; }
    [Required(AllowEmptyStrings = true)] public required string LastName { get; init; }
    public int? Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}