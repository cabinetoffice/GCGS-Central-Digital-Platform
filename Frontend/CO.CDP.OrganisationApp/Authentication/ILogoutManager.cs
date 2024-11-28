namespace CO.CDP.OrganisationApp.Authentication;

public interface ILogoutManager
{
    Task MarkAsLoggedOut(string userUrn, string logout_token);

    Task RemoveAsLoggedOut(string userUrn);

    Task<bool> HasLoggedOut(string userUrn);
}