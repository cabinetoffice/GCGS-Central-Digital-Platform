using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class Tenant : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string Name { get; set; }
    public List<Organisation> Organisations { get; } = [];
    public List<Person> Persons { get; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}