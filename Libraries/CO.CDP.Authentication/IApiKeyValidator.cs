namespace CO.CDP.Authentication;

public interface IApiKeyValidator
{
    Task<(bool valid, Guid? organisation, List<string> scopes)> Validate(string? apiKey);
}