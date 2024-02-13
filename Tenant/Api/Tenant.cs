using System.ComponentModel.DataAnnotations;

namespace Tenant.Api
{
    internal record Tenant
    {
        [Required(AllowEmptyStrings = true)]
        public required string Id { get; init; }
    
        [Required(AllowEmptyStrings = true)]
        public required string Name { get; init; }
    }
}