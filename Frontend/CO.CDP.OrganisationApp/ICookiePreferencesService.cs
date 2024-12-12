namespace CO.CDP.OrganisationApp;

public interface ICookiePreferencesService
{
    bool IsAccepted();
    bool IsRejected();
    void Accept();
    void Reject();
    void Reset();
    bool IsUnknown();
    CookieAcceptanceValues GetValue();
}