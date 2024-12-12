using CO.CDP.EntityVerification.Persistence;
using static CO.CDP.EntityVerification.UseCase.GetIdentifierRegistriesUseCase.GetIdentifierRegistriesException;

namespace CO.CDP.EntityVerification.UseCase;

public class GetIdentifierRegistriesDetailsUseCase(IPponRepository repository) : IUseCase<string[], IEnumerable<Model.IdentifierRegistries>>
{
    public async Task<IEnumerable<Model.IdentifierRegistries>> Execute(string[] schemecodes)
    {
        var rawIdentifiers = await repository.GetIdentifierRegistriesNameAsync(schemecodes);

        var identifiers = rawIdentifiers.Select(identifier => new Model.IdentifierRegistries
        {
            Countrycode = identifier.CountryCode,
            Scheme = identifier.Scheme,
            RegisterName = identifier.RegisterName
        });
        return identifiers;
    }
}