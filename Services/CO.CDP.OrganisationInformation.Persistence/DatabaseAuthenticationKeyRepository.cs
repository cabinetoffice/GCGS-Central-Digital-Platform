using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseAuthenticationKeyRepository(OrganisationInformationContext context) : IAuthenticationKeyRepository
{
    public async Task<AuthenticationKey?> Find(string key)
    {
        return await context.AuthenticationKeys.FirstOrDefaultAsync(t => t.Key == key);
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
}