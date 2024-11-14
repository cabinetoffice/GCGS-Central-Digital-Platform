using CO.CDP.EntityVerification.Persistence;

namespace CO.CDP.EntityVerification.UseCase;

public class GetCountryIdentifiersUseCase(IPponRepository repository) : IUseCase<string, IEnumerable<Model.CountryIdentifiers>>
{
    public async Task<IEnumerable<Model.CountryIdentifiers>> Execute(string countryCode)
    {
        var rawIdentifiers = await repository.GetCountryIdentifiersAsync(countryCode);

        var identifiers = rawIdentifiers.Select(identifier => new Model.CountryIdentifiers
        {
            CountryCode = identifier.CountryCode,
            Scheme = identifier.Scheme,
            RegisterName = identifier.RegisterName
        });

        return identifiers;
    }
}