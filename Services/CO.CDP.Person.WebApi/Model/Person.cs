using System.ComponentModel.DataAnnotations;
namespace CO.CDP.Person.WebApi.Model;

public record Person
{
    [Required(AllowEmptyStrings = true)] public Guid Id { get; init; }
    [Required(AllowEmptyStrings = true)] public string FirstName { get; init; } = string.Empty;
    [Required(AllowEmptyStrings = true)] public string LastName { get; init; } = string.Empty;
    [EmailAddress] public string Email { get; init; } = string.Empty;
    public List<string>? Scopes { get; init; }
}

public record BulkLookupPersonResult
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }
    [Required(AllowEmptyStrings = true)] public required string FirstName { get; init; }
    [Required(AllowEmptyStrings = true)] public required string LastName { get; init; }
    [EmailAddress] public required string Email { get; init; }
    public List<string>? Scopes { get; init; }
}
