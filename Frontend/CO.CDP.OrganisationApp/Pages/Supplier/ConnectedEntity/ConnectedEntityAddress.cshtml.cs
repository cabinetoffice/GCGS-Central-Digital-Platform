using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityAddressModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty(SupportsGet = true)]
    public AddressType AddressType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();

    public string? Caption { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state, true);
        ConnectedEntityState.Address? stateAddress = null;
        if (AddressType == AddressType.Registered) stateAddress = state.RegisteredAddress;
        if (AddressType == AddressType.Postal) stateAddress = state.PostalAddress;

        if (stateAddress != null && stateAddress.IsNonUk == Address.IsNonUkAddress)
        {
            Address.AddressLine1 = stateAddress.AddressLine1;
            Address.TownOrCity = stateAddress.TownOrCity;
            Address.Postcode = stateAddress.Postcode;
            Address.Country = stateAddress.Country;
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);

        if (!ModelState.IsValid) return Page();

        var address = new ConnectedEntityState.Address()
        {
            AddressLine1 = Address.AddressLine1,
            TownOrCity = Address.TownOrCity,
            Postcode = Address.Postcode,
            Country = Address.Country
        };

        if (AddressType == AddressType.Registered)
        {
            state.RegisteredAddress = address;
        }
        else if (AddressType == AddressType.Postal)
        {
            state.PostalAddress = address;
        }
        session.Set(Session.ConnectedPersonKey, state);

        var redirectPage = GetRedirectLinkPageName(state);

        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId });        
    }

    private string GetRedirectLinkPageName(ConnectedEntityState state)
    {
        var redirectPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = (AddressType == AddressType.Postal && state.PostalAddress != null) ?
                            "ConnectedEntityCompanyQuestion" : "ConnectedEntityPostalSameAsRegisteredAddress";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                redirectPage= "ConnectedEntityControlCondition";
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                break;
        }

        return redirectPage;
    }

    private (bool valid, ConnectedEntityState state) ValidatePage()
    {
        var cp = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (cp == null ||
            cp.SupplierOrganisationId != Id ||
            (ConnectedEntityId.HasValue && cp.ConnectedEntityId.HasValue && cp.ConnectedEntityId != ConnectedEntityId))
        {
            return (false, new());
        }
        return (true, cp);
    }

    private void InitModal(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        var heading = "";
        var hintValue = "";
        switch (state.ConnectedEntityType)
        {
            case ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.RegisteredCompany:
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        if (AddressType == AddressType.Registered)
                        {
                            heading = $"Enter {state.OrganisationName}'s registered address";
                            hintValue = "The address registered with Companies House, or the principal address the business conducts its activities. For example, a head office.";
                        }
                        else if (AddressType == AddressType.Postal)
                        {
                            heading = $"Enter {state.OrganisationName}'s postal address";
                        }
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        if (AddressType == AddressType.Registered)
                        {
                            heading = $"Enter {state.OrganisationName}'s last known registered UK address";
                        }
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        heading = $"Enter {state.OrganisationName}'s address";
                        break;
                }
                break;
            case ConnectedEntityType.Individual:
                if (AddressType == AddressType.Registered)
                {
                    heading = $"Enter {state.OrganisationName}'s registered address";
                    hintValue = "The address registered with Companies House, or the principal address the business conducts its activities. For example, a head office.";
                }
                else if (AddressType == AddressType.Postal)
                {
                    heading = $"Enter {state.OrganisationName}'s postal address";
                }
                break;              
            case ConnectedEntityType.TrustOrTrustee:
                //TODO 3: modify value of heading & hint when working on Trust
                break;
        }

        Address.Heading = heading;
        Address.AddressHint = hintValue;
        Address.NonUkAddressLink = $"/organisation/{Id}/supplier-information/connected-person/{AddressType.ToString().ToLower()}-address/non-uk";
    }
}