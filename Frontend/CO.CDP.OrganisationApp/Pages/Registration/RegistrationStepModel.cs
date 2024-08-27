using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public abstract class RegistrationStepModel : LoggedInUserAwareModel
{
    public const string OrganisationTypePage = "/registration/organisation-type";
    public const string OrganisationHasCompanyHouseNumberPage = "/registration/has-companies-house-number";
    public const string OrganisationIdentifierPage = "/registration/organisation-identification";
    public const string OrganisationNamePage = "/registration/organisation-name";
    public const string OrganisationEmailPage = "/registration/organisation-email";
    public const string OrganisationAddressPage = "/registration/organisation-registered-address/uk";
    public const string OrganisationNonUKAddressPage = "/registration/organisation-registered-address/non-uk";
    public const string OrganisationSummaryPage = "/registration/organisation-details-summary";
    
    public const string BuyerOrganisationTypePage = "/registration/buyer-organisation-type";
    public const string BuyerDevolvedRegulationPage = "/registration/buyer-devolved-regulations";
    public const string BuyerSelectDevolvedRegulationPage = "/registration/buyer-select-devolved-regulations";

    public abstract string CurrentPage { get; }

    public string ToRedirectPageUrl { get; protected set; } = "/";

    public RegistrationDetails RegistrationDetails { get; }

    protected RegistrationStepModel()
    {
        RegistrationDetails = SessionContext.Get<RegistrationDetails?>(Session.RegistrationDetailsKey) ?? new();
    }

    public bool ValidateStep()
    {
        return CurrentPage switch
        {
            OrganisationTypePage => true,
            OrganisationHasCompanyHouseNumberPage or OrganisationIdentifierPage => ValidType(),
            OrganisationNamePage => ValidType() && ValidIdentifier(),
            OrganisationEmailPage => ValidType() && ValidIdentifier() && ValidName(),
            OrganisationAddressPage or OrganisationNonUKAddressPage => ValidType() && ValidIdentifier() && ValidName() && ValidEmail(),
            BuyerOrganisationTypePage => ValidType(OrganisationType.Buyer) && ValidIdentifier() && ValidName() && ValidEmail() && ValidAddress(),
            BuyerDevolvedRegulationPage => ValidType(OrganisationType.Buyer) && ValidIdentifier() && ValidName() && ValidEmail() && ValidBuyerOrganisationType(),
            BuyerSelectDevolvedRegulationPage => ValidType(OrganisationType.Buyer) && ValidIdentifier() && ValidName() && ValidEmail() && ValidBuyerOrganisationType() && ValidBuyerDevolvedRegulationsSelected(),
            OrganisationSummaryPage => ValidOrganisationSummary(),
            _ => false,
        };
    }

    private bool ValidOrganisationSummary()
    {
        return RegistrationDetails.OrganisationType switch
        {
            OrganisationType.Supplier => ValidType() && ValidIdentifier() && ValidName() && ValidEmail() && ValidAddress(),
            OrganisationType.Buyer => ValidType() && ValidIdentifier() && ValidName() && ValidEmail() && ValidAddress()
                                    && ValidBuyerOrganisationType() && ValidBuyerDevolvedRegulations(),
            _ => false,
        };
    }

    private bool ValidType(OrganisationType? type = null)
    {
        if (RegistrationDetails.OrganisationType == null || (type.HasValue && RegistrationDetails.OrganisationType != type))
        {
            ToRedirectPageUrl = OrganisationTypePage;
            return false;
        }

        return true;
    }

    private bool ValidIdentifier()
    {
        if (RegistrationDetails.OrganisationScheme == null)
        {
            ToRedirectPageUrl = OrganisationHasCompanyHouseNumberPage;
            return false;
        }
        return true;
    }

    private bool ValidName()
    {
        if (RegistrationDetails.OrganisationName == null)
        {
            ToRedirectPageUrl = OrganisationNamePage;
            return false;
        }
        return true;
    }

    private bool ValidEmail()
    {
        if (RegistrationDetails.OrganisationEmailAddress == null)
        {
            ToRedirectPageUrl = OrganisationEmailPage;
            return false;
        }
        return true;
    }

    private bool ValidAddress()
    {
        if (RegistrationDetails.OrganisationAddressLine1 == null
            || RegistrationDetails.OrganisationCityOrTown == null
            || RegistrationDetails.OrganisationPostcode == null
            || RegistrationDetails.OrganisationCountryCode == null)
        {
            ToRedirectPageUrl = OrganisationAddressPage;
            return false;
        }
        return true;
    }

    private bool ValidBuyerOrganisationType()
    {
        if (RegistrationDetails.BuyerOrganisationType == null)
        {
            ToRedirectPageUrl = BuyerOrganisationTypePage;
            return false;
        }
        return true;
    }

    private bool ValidBuyerDevolvedRegulationsSelected()
    {
        if (RegistrationDetails.Devolved != true)
        {
            ToRedirectPageUrl = BuyerDevolvedRegulationPage;
            return false;
        }
        return true;
    }

    private bool ValidBuyerDevolvedRegulations()
    {
        if (RegistrationDetails.Devolved == null || (RegistrationDetails.Devolved == true && RegistrationDetails.Regulations.Count == 0))
        {
            ToRedirectPageUrl = BuyerDevolvedRegulationPage;
            return false;
        }
        return true;
    }
}