using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Person.Persistence;

public class PersonContext(string connectionString) : DbContext
{
    public DbSet<Person> Persons { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }
}