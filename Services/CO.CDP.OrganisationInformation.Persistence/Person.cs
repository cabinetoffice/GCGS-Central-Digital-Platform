using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Person
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public int? Age { get; set; }
    public required string Email { get; set; }
}