using Microsoft.Extensions.Configuration;

namespace CO.CDP.Authentication;

public class ApiKeyValidator(IConfiguration configuration) : IApiKeyValidator
{
    public Task<bool> Validate(string? apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        var cdpApiKeys = configuration.GetSection("CdpApiKeys").Get<string[]>()
            ?? throw new Exception("Missing configuration key: CdpApiKeys");

        return Task.FromResult(cdpApiKeys.Contains(apiKey));
    }
}