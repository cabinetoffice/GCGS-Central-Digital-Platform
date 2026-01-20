namespace CO.CDP.ApplicationRegistry.App.Authentication;

public interface ITokenService
{
    Task<string?> GetTokenAsync(string tokenName);
}
