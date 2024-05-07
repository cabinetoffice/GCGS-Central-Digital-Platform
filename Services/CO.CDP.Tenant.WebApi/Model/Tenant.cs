using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Tenant.WebApi.Model;

public record Tenant
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }

    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    [Required(AllowEmptyStrings = true)] public required TenantContactInfo ContactInfo { get; init; }
}

public record TenantContactInfo
{
    [Required(AllowEmptyStrings = true), EmailAddress]
    public required string Email { get; init; }

    public required string Phone { get; init; }
}