using System.ComponentModel.DataAnnotations;
namespace CO.CDP.Person.WebApi.Model;

public record Person
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }
    [Required(AllowEmptyStrings = true)] public required string FirstName { get; init; }
    [Required(AllowEmptyStrings = true)] public required string LastName { get; init; }
    [EmailAddress] public required string Email { get; init; }
}