namespace CO.CDP.Authentication;

public interface IApiKeyValidator
{
    Task<(bool valid, int? organisation, List<string> scopes)> Validate(string? apiKey);
}