
using Microsoft.EntityFrameworkCore;
using static CO.CDP.Organisation.Persistence.IOrganisationRepository.OrganisationRepositoryException;

namespace CO.CDP.Organisation.Persistence;
public class DatabaseOrganisationRepository(OrganisationContext context) : IOrganisationRepository
{
    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<Organisation?> Find(Guid organisationId)
    {
        return await context.Organisations.FirstOrDefaultAsync(t => t.Guid == organisationId);
    }

    public async Task<Organisation?> FindByName(string name)
    {
        return await context.Organisations.FirstOrDefaultAsync(t => t.Name == name);
    }

    public void Save(Organisation organisation)
    {
        try
        {
            context.Update(organisation);
            context.SaveChanges();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(organisation, cause);
        }
    }

    private static void HandleDbUpdateException(Organisation organisation, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.ContainsDuplicateKey("_Organisations_Name"):
                throw new DuplicateOrganisationException($"Organisation with name `{organisation.Name}` already exists.", cause);
            case { } e when e.ContainsDuplicateKey("_Organisation_Guid"):
                throw new DuplicateOrganisationException($"Organisation with guid `{organisation.Guid}` already exists.", cause);
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
