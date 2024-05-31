using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public abstract class RegistrationStepModel : PageModel
{
    protected const string UserInfoPage = "/one-login/user-info";
    protected const string OrganisationTypePage = "/registration/organisation-type";
    protected const string OrganisationHasCompanyHouseNumberPage = "/registration/has-companies-house-number";
    protected const string OrganisationIdentifierPage = "/registration/organisation-identification";
    protected const string OrganisationNamePage = "/registration/organisation-name";
    protected const string OrganisationEmailPage = "/registration/organisation-email";
    protected const string OrganisationAddressPage = "/registration/organisation-registered-address";
    protected const string OrganisationNonUKAddressPage = "/registration/organisation-non-uk-address";
    protected const string OrganisationSummaryPage = "/registration/organisation-details-summary";

    public abstract string CurrentPage { get; }

    public abstract ISession SessionContext { get; }

    public string ToRedirectPageUrl { get; protected set; } = "/";

    public RegistrationDetails RegistrationDetails { get; } = new();

    public UserDetails? UserDetails { get; }

    protected RegistrationStepModel()
    {
        RegistrationDetails = SessionContext.Get<RegistrationDetails?>(Session.RegistrationDetailsKey) ?? new();
        UserDetails = SessionContext.Get<UserDetails>(Session.UserDetailsKey);
    }

    public bool ValidateStep()
    {
        ToRedirectPageUrl = "/";
        if (UserDetails == null)
        {
            ToRedirectPageUrl = UserInfoPage;
            return false;
        }

        var returnVal = true;

        return CurrentPage switch
        {
            OrganisationHasCompanyHouseNumberPage or OrganisationIdentifierPage => ValidType(),
            OrganisationNamePage => ValidType() && ValidIdentifier(),
            OrganisationEmailPage => ValidType() && ValidIdentifier() && ValidName(),
            OrganisationAddressPage or OrganisationNonUKAddressPage => ValidType() && ValidIdentifier() && ValidName() && ValidEmail(),
            OrganisationSummaryPage => ValidType() && ValidIdentifier() && ValidName() && ValidEmail() && ValidAddress(),
            _ => returnVal,
        };
    }

    private bool ValidType()
    {
        if (RegistrationDetails?.OrganisationType == null)
        {
            ToRedirectPageUrl = OrganisationTypePage;
            return false;
        }
        return true;
    }

    private bool ValidIdentifier()
    {
        if (RegistrationDetails?.OrganisationScheme == null
            || RegistrationDetails?.OrganisationIdentificationNumber == null)
        {
            ToRedirectPageUrl = OrganisationHasCompanyHouseNumberPage;
            return false;
        }
        return true;
    }

    private bool ValidName()
    {
        if (RegistrationDetails?.OrganisationName == null)
        {
            ToRedirectPageUrl = OrganisationNamePage;
            return false;
        }
        return true;
    }

    private bool ValidEmail()
    {
        if (RegistrationDetails?.OrganisationEmailAddress == null)
        {
            ToRedirectPageUrl = OrganisationEmailPage;
            return false;
        }
        return true;
    }

    private bool ValidAddress()
    {
        if (RegistrationDetails?.OrganisationAddressLine1 == null
            || RegistrationDetails?.OrganisationCityOrTown == null
            || RegistrationDetails?.OrganisationPostcode == null
            || RegistrationDetails?.OrganisationCountry == null)
        {
            ToRedirectPageUrl = OrganisationAddressPage;
            return false;
        }
        return true;
    }
}