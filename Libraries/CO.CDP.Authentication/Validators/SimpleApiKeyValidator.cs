namespace CO.CDP.Authentication;

public class SimpleApiKeyValidator
{
    public Task<bool> Validate(string? apiKey, string? configuredApiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(configuredApiKey))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(apiKey == configuredApiKey);
    }
}
