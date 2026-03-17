namespace CO.CDP.UserManagement.App.Authentication;

public interface IOneLoginAuthority
{
    Task<string?> ValidateLogoutToken(string logoutToken);
}
