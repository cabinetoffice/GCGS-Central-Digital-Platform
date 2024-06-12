using Microsoft.EntityFrameworkCore;
using CO.CDP.Tenant.WebApi.Model;

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

    public async Task<UserTenantLookup?> FindByUserUrn(string userUrn)
    {
        return await context.Persons
            .Where(p => p.UserUrn == userUrn)
            .Select(p => new UserTenantLookup
            {
                User = new UserTenantLookup.PersonUser
                {
                    Email = p.Email,
                    Urn = p.UserUrn ?? "",
                    Name = $"{p.FirstName} {p.LastName}"
                },
                Tenants = p.Tenants.Select(t => new UserTenantLookup.Tenant
                {
                    Id = t.Guid,
                    Name = t.Name,
                    Organisations = t.Organisations.Select(o => new UserTenantLookup.Organisation
                    {
                        Id = o.Guid,
                        Name = o.Name,
                        Roles = o.Roles,
                        Scopes = o.OrganisationPersons.Single(op => op.PersonId == p.Id).Scopes
                    }).ToList()
                }).ToList()
            })
            .SingleAsync();
    }

    public async Task<Person?> FindByUrn(string urn)
    {
        return await context.Persons.FirstOrDefaultAsync(t => t.UserUrn == urn);
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
