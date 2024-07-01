namespace CO.CDP.Organisation.WebApi.ApiKeyAuthentication;

public class ApiKeyValidator : IApiKeyValidator
{
    public Task<bool> Validate(string apiKey)
    {
        return Task.FromResult(apiKey == "5607c66c-2c31-4d66-ac6e-3c0c5e9150a9");
    }
}