using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

public abstract class ConsortiumStepModel : LoggedInUserAwareModel
{
    public const string ConsortiumStartPage = "/consortium/start";
    public const string ConsortiumNamePage = "/consortium/consortium-name";
    public const string ConsortiumAddressPage = "/consortium/address/uk";
    public const string ConsortiumNonUKAddressPage = "/consortium/address/non-uk";
    public const string ConsortiumEmailPage = "/consortium/email";
    public const string ConsortiumOverviewPage = "/consortium/overview";

    public abstract string CurrentPage { get; }

    public string ToRedirectPageUrl { get; protected set; } = "/";

    public ConsortiumDetails ConsortiumDetails { get; }

    protected ConsortiumStepModel(ISession session) : base(session)
    {
        ConsortiumDetails = SessionContext.Get<ConsortiumDetails?>(Session.ConsortiumKey) ?? new();
    }

    public bool ValidateStep()
    {
        return CurrentPage switch
        {
            ConsortiumStartPage => true,
            ConsortiumNamePage => true,
            ConsortiumAddressPage or ConsortiumNonUKAddressPage => ValidName(),
            ConsortiumEmailPage => ValidName() && ValidAddress(),
            ConsortiumOverviewPage => true,
            _ => false,
        };
    }

    private bool ValidName()
    {
        if (ConsortiumDetails.ConstortiumName == null)
        {
            ToRedirectPageUrl = ConsortiumNamePage;
            return false;
        }
        return true;
    }

    private bool ValidEmail()
    {
        if (ConsortiumDetails.ConstortiumEmail == null)
        {
            ToRedirectPageUrl = ConsortiumEmailPage;
            return false;
        }
        return true;
    }

    private bool ValidAddress()
    {
        if (ConsortiumDetails.PostalAddress != null
            && (ConsortiumDetails.PostalAddress.AddressLine1 == null
            || ConsortiumDetails.PostalAddress.TownOrCity == null
            || ConsortiumDetails.PostalAddress.Postcode == null
            || ConsortiumDetails.PostalAddress.Country == null))
        {
            ToRedirectPageUrl = ConsortiumAddressPage;
            return false;
        }
        return true;
    }
}