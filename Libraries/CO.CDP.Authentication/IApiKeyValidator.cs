namespace CO.CDP.Authentication;

public interface IApiKeyValidator
{
    Task<bool> Validate(string? apiKey);
}
