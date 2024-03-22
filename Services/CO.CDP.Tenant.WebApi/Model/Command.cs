using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Tenant.WebApi.Model;

public record RegisterTenant
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    [Required(AllowEmptyStrings = true)] public required TenantContactInfo ContactInfo { get; init; }
}

internal record UpdateTenant
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    [Required(AllowEmptyStrings = true)] public required TenantContactInfo ContactInfo { get; init; }
}