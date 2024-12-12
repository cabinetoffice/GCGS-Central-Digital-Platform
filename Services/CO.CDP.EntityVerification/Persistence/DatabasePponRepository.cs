using CO.CDP.EntityFrameworkCore.DbContext;
using CO.CDP.EntityVerification.Events;
using Microsoft.EntityFrameworkCore;
using static CO.CDP.EntityVerification.Persistence.IPponRepository.PponRepositoryException;

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
            context.Remove(identifier);
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

    public async Task<IEnumerable<IdentifierRegistries>> GetIdentifierRegistriesAsync(string countryCode)
    {
        return await context.IdentifierRegistries.Where(q => q.CountryCode.ToUpper() == countryCode.ToUpper()).ToListAsync();
    }

    public async Task<IEnumerable<IdentifierRegistries>> GetIdentifierRegistriesNameAsync(string[] schemeCodes)
    {
        var upperCaseSchemeCodes = schemeCodes.Select(x => x.ToUpper()).ToArray(); 

        return await context.IdentifierRegistries
            .Where(q => upperCaseSchemeCodes.Contains(q.Scheme.ToUpper())) 
            .ToListAsync();
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
}