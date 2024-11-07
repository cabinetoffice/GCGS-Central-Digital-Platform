namespace CO.CDP.OrganisationApp.Authentication;

public interface IOneLoginSessionManager
{
    void AddUserLoggedOut(string sub);

    void RemoveUserLoggedOut(string sub);

    bool IsLoggedOut(string sub);
}