using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Person.WebApi.Model;

public record RegisterPerson
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    public required int Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}

internal record UpdatePerson
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    public required int Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}