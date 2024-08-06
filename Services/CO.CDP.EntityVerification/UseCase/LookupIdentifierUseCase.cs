using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification.UseCase;

public class LookupIdentifierUseCase(IPponRepository repo) : IUseCase<LookupIdentifierQuery, IEnumerable<Model.Identifier>>
{
    public async Task<IEnumerable<Model.Identifier>> Execute(LookupIdentifierQuery query)
    {
        List<Model.Identifier> foundIdentifiers = [];

        if (query.TryGetIdentifier(out var scheme, out var id))
        {
            var ppon = await repo.FindPponByIdentifierAsync(scheme, id);

            if (ppon != null)
            {
                foundIdentifiers = ppon.Identifiers.Select(item => new Model.Identifier
                {
                    Id = item.IdentifierId,
                    LegalName = item.LegalName,
                    Scheme = item.Scheme,
                    Uri = item.Uri
                }).ToList();
            }
        }
        else
        {
            throw new InvalidIdentifierFormatException("Both scheme and identifier are required in the format: scheme:identifier");
        }

        return foundIdentifiers;
    }

    public abstract class LookupIdentifierException(string message, Exception? cause = null)
        : Exception(message, cause)
    {
        public class InvalidIdentifierFormatException(string message, Exception? cause = null) : Exception(message, cause);
    }
}