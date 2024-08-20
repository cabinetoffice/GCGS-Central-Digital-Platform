using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

public class DatabaseAuthorityRepository(OrganisationInformationContext context) : IAuthorityRepository
{
    public async Task<RefreshToken?> Find(string tokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return await context.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                rt.ExpiryDate > DateTime.UtcNow &&
                (rt.Revoked ?? false) == false);
    }

    public async Task Save(RefreshToken refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        context.Update(refreshToken);
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}