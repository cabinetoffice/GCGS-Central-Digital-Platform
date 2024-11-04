using Microsoft.EntityFrameworkCore;

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

    public async Task<Person?> FindByUrn(string urn)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.UserUrn == urn);
    }

    public async Task<Person?> FindPersonWithTenant(Guid personId)
    {
        return await context.Persons
            .Include(p => p.Tenants)
            .Where(p => p.Tenants.Count != 0)
            .FirstOrDefaultAsync(o => o.Guid == personId);
    }

    public async Task<IEnumerable<Person>> FindByOrganisation(Guid organisationId)
    {
        var organisation = await context.Organisations
            .Include(o => o.Persons)
            .Where(o => o.Guid == organisationId)
            .FirstOrDefaultAsync();
        return organisation?.Persons ?? [];
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
            case { } e when e.Message.Contains("_persons_email"):
                throw new IPersonRepository.PersonRepositoryException.DuplicatePersonException(
                    $"Person with email `{person.Email}` already exists.", cause);
            case { } e when e.Message.Contains("_persons_guid"):
                throw new IPersonRepository.PersonRepositoryException.DuplicatePersonException(
                    $"Person with guid `{person.Guid}` already exists.", cause);
            default:
                throw cause;
        }
    }


}