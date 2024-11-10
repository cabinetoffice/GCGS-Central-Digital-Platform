namespace CO.CDP.OrganisationApp.Authentication;

public interface ITokenService
{
    Task<string?> GetTokenAsync(string tokenName);
}