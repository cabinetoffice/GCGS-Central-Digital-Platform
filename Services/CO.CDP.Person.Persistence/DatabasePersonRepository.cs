using Microsoft.EntityFrameworkCore;
using static CO.CDP.Person.Persistence.IPersonRepository.PersonRepositoryException;

namespace CO.CDP.Person.Persistence;

public class DatabasePersonRepository(PersonContext context) : IPersonRepository
{
    public void Save(Person person)
    {
        try
        {
            context.Update(person);
            context.SaveChanges();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(person, cause);
        }
    }

    public async Task<Person?> Find(Guid personId)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.Guid == personId);
    }

    public async Task<Person?> FindByName(string name)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<Person?> FindByEmail(string emailAddress)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.Email == emailAddress);
    }

    public void Dispose()
    {
        context.Dispose();
    }

    private static void HandleDbUpdateException(Person person, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.ContainsDuplicateKey("_Persons_Name"):
                throw new DuplicatePersonException($"Person with name `{person.Name}` already exists.", cause);
            case { } e when e.ContainsDuplicateKey("_Persons_Guid"):
                throw new DuplicatePersonException($"Person with guid `{person.Guid}` already exists.", cause);
            default: throw cause;
        }
    }
}

internal static class StringExtensions
{
    internal static bool ContainsDuplicateKey(this Exception cause, string name) =>
        cause.Message.ContainsDuplicateKey(name);

    private static bool ContainsDuplicateKey(this string message, string name) =>
        message.Contains("duplicate key value violates unique constraint") &&
        message.Contains($"{name}\"");
}
