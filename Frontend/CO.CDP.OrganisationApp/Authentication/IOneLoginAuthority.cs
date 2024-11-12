namespace CO.CDP.OrganisationApp.Authentication;

public interface IOneLoginAuthority
{
    Task<string?> ValidateLogoutToken(string logoutToken);
}