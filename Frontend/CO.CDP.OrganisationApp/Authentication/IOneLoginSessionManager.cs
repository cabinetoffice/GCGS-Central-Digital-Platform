namespace CO.CDP.OrganisationApp.Authentication;

public interface IOneLoginSessionManager
{
    Task AddToSignedOutSessionsList(string userUrn);

    Task RemoveFromSignedOutSessionsList(string userUrn);

    Task<bool> HasSignedOut(string userUrn);
}