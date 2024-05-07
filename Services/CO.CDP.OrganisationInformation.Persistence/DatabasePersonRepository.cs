using Microsoft.EntityFrameworkCore;
using static CO.CDP.OrganisationInformation.Persistence.IPersonRepository.PersonRepositoryException;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabasePersonRepository(OrganisationInformationContext context) : IPersonRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<Person?> Find(Guid personId)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.Guid == personId);
    }

    public async Task<Person?> FindByEmail(string email)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.Email == email);
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
                throw new IPersonRepository.PersonRepositoryException.DuplicatePersonException($"Person with email `{person.Email}` already exists.", cause);
            case { } e when e.Message.Contains("_Persons_Guid"):
                throw new IPersonRepository.PersonRepositoryException.DuplicatePersonException($"Person with guid `{person.Guid}` already exists.", cause);
            default:
                throw cause;
        }
    }
}