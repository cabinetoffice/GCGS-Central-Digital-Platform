using CO.CDP.EntityVerification.Events;
using CO.CDP.EntityVerification.Model;
using CO.CDP.EntityVerification.Persistence;
using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

namespace CO.CDP.EntityVerification.UseCase;

public class GetCountryIdentifiersUseCase(IPponRepository repo) : IUseCase<string, IEnumerable<Model.CountryIdentifiers>>
{
    public async Task<IEnumerable<Model.CountryIdentifiers>> Execute(string countryCode)
    {
        throw new NotImplementedException();
    }   
}