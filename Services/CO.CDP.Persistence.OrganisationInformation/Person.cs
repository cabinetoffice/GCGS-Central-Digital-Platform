using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Persistence.OrganisationInformation;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Person
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public required string Email { get; set; }
}