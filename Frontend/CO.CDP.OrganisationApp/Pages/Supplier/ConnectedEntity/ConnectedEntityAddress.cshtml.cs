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

    public string? Caption
    {
        get
        {
            return ""; //TODO 1: populate values;
        }
    }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntityQuestion", new { Id, ConnectedEntityId });
        }
        ConnectedEntityState.Address? stateAddress = null;
        if (AddressType == AddressType.Registered) stateAddress = state.RegisteredAddress;
        if (AddressType == AddressType.Postal) stateAddress = state.PostalAddress;

        if (stateAddress != null)
        {
            Address.AddressLine1 = stateAddress.AddressLine1;
            Address.TownOrCity = stateAddress.TownOrCity;
            Address.Postcode = stateAddress.Postcode;
            Address.Country = stateAddress.Country;
        }

        SetupAddress(true);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntityQuestion", new { Id, ConnectedEntityId });
        }

        SetupAddress();

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

        //TODO 2: different page if different indivisual/trust category;
        return RedirectToPage("ConnectedEntityPostalSameAsRegisteredAddress", new { Id, ConnectedEntityId });
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

    public enum CategoryBlah
    {
        Cat1,
        Cat2,
        Cat3
    }

    //TODO 3: modify value of heading & hint
    private void SetupAddress(bool reset = false)
    {
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        CategoryBlah ss = CategoryBlah.Cat1;
        var heading = "";
        var hintValue = "";
        switch (ss)
        {
            case CategoryBlah.Cat1:
                if (AddressType == AddressType.Registered)
                {
                    heading = "Enter {sadsa's} registered address";
                    hintValue = "The address registered with the equivalent to Companies House, or the principal address the business conducts its activities. For example, a head office.";
                }
                else if (AddressType == AddressType.Postal)
                {
                }
                break;
            case CategoryBlah.Cat2:
                break;
            case CategoryBlah.Cat3:
                break;
            default:
                break;
        }

        Address.Heading = heading;
        Address.AddressHint = hintValue;
        Address.NonUkAddressLink = $"/organisation/{Id}/supplier-information/connected-person/{AddressType.ToString().ToLower()}-address/non-uk";
    }
}