using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseAuthenticationKeyRepository(OrganisationInformationContext context) : IAuthenticationKeyRepository
{
    public async Task<AuthenticationKey?> Find(string key)
    {
        return await context.AuthenticationKeys.Include(a => a.Organisation).FirstOrDefaultAsync(t => t.Key == key && t.Revoked == false);
    }

    public async Task Save(AuthenticationKey authenticationKey)
    {
        try
        {
            context.Update(authenticationKey);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(authenticationKey, cause);
        }
    }

    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<IEnumerable<AuthenticationKey?>> GetAuthenticationKeys(Guid organisationId)
    {
        return await context.AuthenticationKeys
            .Include(a => a.Organisation)
            .Where(t => t.Organisation!.Guid == organisationId)
            .ToArrayAsync();
    }
    public async Task<AuthenticationKey?> FindByKeyNameAndOrganisationId(string name, Guid organisationId)
    {
        return await context.AuthenticationKeys
            .Include(a => a.Organisation)
            .Where(t => t.Organisation!.Guid == organisationId)
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    private static void HandleDbUpdateException(AuthenticationKey authenticationKey, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.ContainsDuplicateKey("_authentication_keys_name_organisation_id"):
                throw new IAuthenticationKeyRepository.AuthenticationKeyRepositoryException.DuplicateAuthenticationKeyNameException(
                    $"Authentication Key with name `{authenticationKey.Name}` already exists.", cause);
            default: throw cause;
        }
    }
}