namespace CO.CDP.Authentication.Services;

public interface ILogoutManager
{
    Task MarkAsLoggedOut(string userUrn, string logoutToken);
    Task RemoveAsLoggedOut(string userUrn);
    Task<bool> HasLoggedOut(string userUrn);
}
