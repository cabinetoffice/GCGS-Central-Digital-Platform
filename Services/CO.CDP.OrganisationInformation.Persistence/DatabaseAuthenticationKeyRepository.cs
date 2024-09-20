using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseAuthenticationKeyRepository(OrganisationInformationContext context) : IAuthenticationKeyRepository
{
    public async Task<AuthenticationKey?> Find(string key)
    {
        return await context.AuthenticationKeys.Include(a => a.Organisation).FirstOrDefaultAsync(t => t.Key == key);
    }

    public async Task Save(AuthenticationKey authenticationKey)
    {
        context.Update(authenticationKey);
        await context.SaveChangesAsync();
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
            .FirstOrDefaultAsync(t => t.Name == name && t.OrganisationId == t.Organisation!.Id);
    }
}