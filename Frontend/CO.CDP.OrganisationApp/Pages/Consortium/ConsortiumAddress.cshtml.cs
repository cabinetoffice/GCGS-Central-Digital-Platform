using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
public class ConsortiumAddressModel(ISession session) : PageModel
{

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();
    public string? ConsortiumName { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();

        if (!valid)
        {
            return RedirectToPage("ConsortiumStart");
        }

        InitModel(state);

        SetupAddress(true);

        var stateAddress = state.PostalAddress!;
        if (stateAddress != null && ((stateAddress.Country != Constants.Country.UKCountryCode) == Address.IsNonUkAddress))
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
        SetupAddress();

        var (valid, state) = ValidatePage();

        if (!valid)
        {
            return RedirectToPage("ConsortiumStart");
        }

        InitModel(state);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var address = new Address()
        {            
            AddressLine1 = Address.AddressLine1!,
            TownOrCity = Address.TownOrCity!,
            Postcode = Address.Postcode!,
            CountryName = Address.CountryName!,
            Country = Address.Country!
        };

        state.PostalAddress = address;

        session.Set(Session.ConsortiumKey, state);

        return RedirectToPage("ConsortiumOverview");

    }

    private void SetupAddress(bool reset = false)
    {
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        if (Address.IsNonUkAddress)
        {
            Address.Heading = StaticTextResource.Consortium_ConsortiumAddress_NonUk_Heading;
            Address.AddressHint = StaticTextResource.Consortium_ConsortiumAddress_NonUk_Hint;
        }
        else
        {
            Address.Heading = StaticTextResource.Consortium_ConsortiumAddress_Heading;
            Address.AddressHint = StaticTextResource.Consortium_ConsortiumAddress_Hint;
        }

        Address.NonUkAddressLink = $"/consortium/address/non-uk";
    }

    private void InitModel(ConsortiumState state)
    {
        ConsortiumName = state.ConstortiumName;
    }

    private (bool valid, ConsortiumState state) ValidatePage()
    {
        var cd = session.Get<ConsortiumState>(Session.ConsortiumKey);

        if (cd == null)
        {
            return (false, new());
        }

        return (true, cd);
    }
}