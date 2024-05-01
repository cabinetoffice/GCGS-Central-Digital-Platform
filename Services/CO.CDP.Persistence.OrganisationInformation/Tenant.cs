using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Persistence.OrganisationInformation;

[Index(nameof(Guid), IsUnique=true)]
[Index(nameof(Name), IsUnique=true)]
public class Tenant
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string Name { get; set; }
    public required TenantContactInfo ContactInfo { get; set; }

    [ComplexType]
    public record TenantContactInfo
    {
        public required string Email;
        public required string Phone;
    }
}