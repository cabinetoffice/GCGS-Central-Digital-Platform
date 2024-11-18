using CO.CDP.EntityVerification.Persistence;
using static CO.CDP.EntityVerification.UseCase.GetCountryIdentifiersUseCase.GetCountryIdentifiersException;

namespace CO.CDP.EntityVerification.UseCase;

public class GetCountryIdentifiersUseCase(IPponRepository repository) : IUseCase<string, IEnumerable<Model.CountryIdentifiers>>
{
    public async Task<IEnumerable<Model.CountryIdentifiers>> Execute(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            throw new InvalidInputException("Country code cannot be null or empty.");
        }

        var rawIdentifiers = await repository.GetCountryIdentifiersAsync(countryCode);

        if (rawIdentifiers == null || !rawIdentifiers.Any())
        {
            throw new NotFoundException($"No identifiers found for country code: {countryCode}");
        }

        var identifiers = rawIdentifiers.Select(identifier => new Model.CountryIdentifiers
        {
            Scheme = identifier.Scheme,
            RegisterName = identifier.RegisterName
        });

        return identifiers;
    }

    public abstract class GetCountryIdentifiersException(string message, Exception? cause = null)
      : Exception(message, cause)
    {
        public class NotFoundException(string message, Exception? cause = null) : Exception(message, cause);
        public class InvalidInputException(string message, Exception? cause = null) : Exception(message, cause);

    }
}