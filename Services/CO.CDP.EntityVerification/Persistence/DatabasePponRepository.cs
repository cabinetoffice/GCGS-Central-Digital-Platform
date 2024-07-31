using Microsoft.EntityFrameworkCore;
using System.Linq;
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
            HandleDbUpdateException(identifier, cause);
        }
    }

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