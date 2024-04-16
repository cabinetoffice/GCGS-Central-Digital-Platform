using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Person.Persistence;

[Index(nameof(Guid), IsUnique=true)]
[Index(nameof(Email), IsUnique=true)]
public class Person
{

    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
