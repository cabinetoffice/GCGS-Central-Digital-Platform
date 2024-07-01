namespace CO.CDP.Organisation.WebApi.ApiKeyAuthentication;

public interface IApiKeyValidator
{
    Task<bool> Validate(string apiKey);
}
