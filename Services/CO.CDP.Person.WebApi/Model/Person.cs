using System.ComponentModel.DataAnnotations;
namespace CO.CDP.Person.WebApi.Model;

public record Person
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    public required int Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}
