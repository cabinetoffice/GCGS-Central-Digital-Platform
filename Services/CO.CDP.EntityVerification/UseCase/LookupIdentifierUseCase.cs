using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using System.Collections.Generic;
using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification.UseCase;

public class LookupIdentifierUseCase : IUseCase<LookupIdentifierQuery, IEnumerable<Model.Identifier>?>
{
    public async Task<IEnumerable<Model.Identifier>?> Execute(LookupIdentifierQuery query)
    {
        IEnumerable<Model.Identifier>? foundIdentifiers = null;

        if (query.TryGetIdentifier(out var scheme, out var id))
        {
            if ((query.Identifier == "CDP-PPON:ac73982be54e456c888d495b6c2c997f") || ((query.Identifier == "GB-COH:9443242")))
            {
                foundIdentifiers = new List<Model.Identifier>
                {
                    new Model.Identifier
                    {
                        Id = "ac73982be54e456c888d495b6c2c997f",
                        LegalName = "Acme",
                        Scheme = "CDP-PPON",
                        Uri = new Uri("https://www.acme-ltd.com")
                    },
                    new Model.Identifier
                    {
                        Id = "12345678",
                        LegalName = "Acme",
                        Scheme = "GB-COH",
                        Uri = new Uri("https://www.acme-ltd.com")
                    }
                };
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