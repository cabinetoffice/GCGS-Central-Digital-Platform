namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
public class OrganisationRegistrationUnavailable(ISession session) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    public void OnGet()
    {
    }
}