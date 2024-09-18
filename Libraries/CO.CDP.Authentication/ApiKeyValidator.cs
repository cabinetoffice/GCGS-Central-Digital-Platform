using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Authentication;

public class ApiKeyValidator(
    IAuthenticationKeyRepository repository) : IApiKeyValidator
{
    public async Task<(bool valid, Guid? organisation, List<string> scopes)> Validate(string? apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        var authKey = await repository.Find(apiKey);

        return (authKey != null, authKey?.Organisation?.Guid, authKey?.Scopes ?? []);
    }
}