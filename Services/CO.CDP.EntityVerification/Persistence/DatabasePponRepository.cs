using Microsoft.EntityFrameworkCore;

namespace CO.CDP.EntityVerification.Persistence;

public class DatabasePponRepository : IPponRepository
{
    public void Save(EntityVerificationContext context, Ppon identifier)
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

    private static void HandleDbUpdateException(Ppon identifier, DbUpdateException cause)
    {
        switch (cause.InnerException)
        {
            default:
                throw cause;
        }
    }
}