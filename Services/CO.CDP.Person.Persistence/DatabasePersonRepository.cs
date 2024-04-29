using Microsoft.EntityFrameworkCore;
using static CO.CDP.Person.Persistence.IPersonRepository.PersonRepositoryException;

namespace CO.CDP.Person.Persistence;
public class DatabasePersonRepository(PersonContext context) : IPersonRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<Person?> Find(Guid organisationId)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Person?> FindByName(string name)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.Name == name);
    }

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

    private static void HandleDbUpdateException(Person person, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.Message.Contains("_Persons_Email"):
                throw new DuplicatePersonException($"Person with email `{person.Email}` already exists.", cause);
            case { } e when e.Message.Contains("_Persons_Guid"):
                throw new DuplicatePersonException($"Person with guid `{person.Guid}` already exists.", cause);
            default:
                throw cause;
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
