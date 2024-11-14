using CO.CDP.EntityFrameworkCore.DbContext;
using CO.CDP.EntityVerification.Events;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.EntityVerification.Persistence.IPponRepository.PponRepositoryException;
using static System.Collections.Specialized.BitVector32;

namespace CO.CDP.EntityVerification.Persistence;

public class DatabasePponRepository(EntityVerificationContext context) : IPponRepository
{
    public void Save(Ppon identifier)
    {
        try
        {
            context.Update(identifier);
            context.SaveChanges();
        }
        catch (DbUpdateException cause)
        {
            HandleDbUpdateException(identifier, cause);
        }
    }

    public async Task SaveAsync(Ppon identifier, Func<Ppon, Task> onSave) =>
        await context.InTransaction(async _ =>
        {
            Save(identifier);
            await onSave(identifier);
            await context.SaveChangesAsync();
        });

    public void Dispose()
    {
        context.Dispose();
    }

    public async Task<Ppon?> FindPponByPponIdAsync(string pponId)
    {
        return await context.Ppons
            .Include(p => p.Identifiers)
            .FirstOrDefaultAsync(p => p.IdentifierId == pponId);
    }

    public async Task<Ppon?> FindPponByIdentifierAsync(string scheme, string id)
    {
        var ppons = await context.Ppons
            .Include(p => p.Identifiers)
            .ToListAsync();

        return ppons.FirstOrDefault(p =>
                p.Identifiers.Any(i => i.IdentifierId == id && i.Scheme == scheme) ||
                (scheme == IdentifierSchemes.Ppon && p.IdentifierId == id));
    }

    private static void HandleDbUpdateException(Ppon identifier, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            case { } e when e.ContainsDuplicateKey("_ppons_identifier_id"):
                throw new DuplicatePponException($"PPON with PPON Id `{identifier.IdentifierId}` already exists.", cause);
            default:
                throw cause;
        }
    }

    public async Task<IEnumerable<CountryIndentifiers>> GetCountryIdentifiersAsync(string countryCode)
    {
        return await context.CountryIdentifiers.Where(q => q.CountryCode == countryCode).ToListAsync();
    }
}