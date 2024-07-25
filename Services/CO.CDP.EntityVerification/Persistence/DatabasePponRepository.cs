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

    public async Task<Ppon?> FindPponByIdentifierAsync(IEnumerable<Events.Identifier> pponIdentifiers)
    {
        var ppons = await context.Ppons
            .Include(p => p.Identifiers)
            .ToListAsync();

        return ppons.FirstOrDefault(p => p.Identifiers.Any(i =>
            pponIdentifiers.Any(pi =>
                i.IdentifierId == pi.Id &&
                i.Scheme == pi.Scheme)));
    }

    public void UpdatePponIdentifiersAsync(Ppon pponToUpdate, IEnumerable<Events.Identifier> identifiers)
    {
        // Find the matching identifiers for the pponToUpdate
        foreach (var idToUpdate in identifiers)
        {
            var existingIdentifier = context.Identifiers.FirstOrDefault(i => i.IdentifierId == idToUpdate.Id && i.Scheme == idToUpdate.Scheme);
            if (existingIdentifier != null)
            {
                existingIdentifier.Uri = idToUpdate.Uri;
                existingIdentifier.LegalName = idToUpdate.LegalName;
            }
        }

        // Find the new identifiers to add.
        var newIdentifiers = identifiers
            .Where(pi => !pponToUpdate.Identifiers.Any(i => i.IdentifierId == pi.Id && i.Scheme == pi.Scheme))
            .ToList();

        foreach (var identifier in Identifier.GetPersistenceIdentifiers(newIdentifiers, pponToUpdate))
        {
            pponToUpdate.Identifiers.Add(identifier);
        }
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