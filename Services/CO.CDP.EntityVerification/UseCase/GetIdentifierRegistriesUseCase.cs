using CO.CDP.EntityVerification.Persistence;
using static CO.CDP.EntityVerification.UseCase.GetIdentifierRegistriesUseCase.GetIdentifierRegistriesException;

namespace CO.CDP.EntityVerification.UseCase;

public class GetIdentifierRegistriesUseCase(IPponRepository repository) : IUseCase<string, IEnumerable<Model.IdentifierRegistries>>
{
    public async Task<IEnumerable<Model.IdentifierRegistries>> Execute(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            throw new InvalidInputException("Country code cannot be null or empty.");
        }

        var rawIdentifiers = await repository.GetIdentifierRegistriesAsync(countryCode);

        if (rawIdentifiers == null || !rawIdentifiers.Any())
        {
            throw new NotFoundException($"No identifiers found for country code: {countryCode}");
        }

        var identifiers = rawIdentifiers.Select(identifier => new Model.IdentifierRegistries
        {
            Countrycode = identifier.CountryCode,
            Scheme = identifier.Scheme,
            RegisterName = identifier.RegisterName
        });

        return identifiers;
    }

    public abstract class GetIdentifierRegistriesException(string message, Exception? cause = null)
      : Exception(message, cause)
    {
        public class NotFoundException(string message, Exception? cause = null) : Exception(message, cause);
        public class InvalidInputException(string message, Exception? cause = null) : Exception(message, cause);

    }
}