using System.ComponentModel.DataAnnotations.Schema;
using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Key), IsUnique = true)]
public class AuthenticationKey : IEntityDate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Key { get; set; }

    [ForeignKey(nameof(Organisation))]
    public int? OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }
    public List<string> Scopes { get; set; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}