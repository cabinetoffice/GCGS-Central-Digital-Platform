using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Tenant.WebApi.Model;

public record Tenant
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }

    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
}