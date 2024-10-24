using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabasePersonInviteRepository(OrganisationInformationContext context) : IPersonInviteRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<PersonInvite?> Find(Guid personInviteId)
    {
        return await context.PersonInvites
            .Include(pi => pi.Organisation)
            .Include(pi => pi.Person)
            .Include(pi => pi.Organisation.Tenant)
            .AsSingleQuery()
            .FirstOrDefaultAsync(t => t.Guid == personInviteId);
    }

    public async Task<IEnumerable<PersonInvite>> FindByOrganisation(Guid organisationId)
    {
        return await context.PersonInvites
            .Include(pi => pi.Person)
            .Where(pi => pi.Organisation.Guid == organisationId)
            .ToArrayAsync();
    }

    public async Task<bool> Delete(PersonInvite personInvite)
    {
        context.Remove(personInvite);
        await context.SaveChangesAsync();

        return true;
    }

    public void Save(PersonInvite personInvite)
    {
        try
        {
            context.Update(personInvite);
            context.SaveChanges();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(personInvite, cause);
        }
    }

    private static void HandleDbUpdateException(PersonInvite personInvite, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.Message.Contains("_persons_email"):
                throw new IPersonRepository.PersonRepositoryException.DuplicatePersonException(
                    $"Person invite with email `{personInvite.Email}` already exists.", cause);
            case { } e when e.Message.Contains("_persons_guid"):
                throw new IPersonRepository.PersonRepositoryException.DuplicatePersonException(
                    $"Person invite with guid `{personInvite.Guid}` already exists.", cause);
            default:
                throw cause;
        }
    }
}