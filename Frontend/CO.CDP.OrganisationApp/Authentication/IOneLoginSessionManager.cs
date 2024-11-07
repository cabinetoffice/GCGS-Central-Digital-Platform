namespace CO.CDP.OrganisationApp.Authentication;

public interface IOneLoginSessionManager
{
    void AddToSignedOutSessionsList(string sub);

    void RemoveFromSignedOutSessionsList(string sub);

    bool HasSignedOut(string sub);
}