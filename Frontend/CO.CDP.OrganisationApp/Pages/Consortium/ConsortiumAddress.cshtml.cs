using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
[ValidateConsortiumStep]
public class ConsortiumAddressModel(ISession session) : ConsortiumStepModel(session)
{
    public override string CurrentPage => ConsortiumAddressPage;

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();
    public string? ConsortiumName => ConsortiumDetails.ConsortiumName;

    public IActionResult OnGet()
    {
        SetupAddress(true);

        var stateAddress = ConsortiumDetails.PostalAddress!;

        if (stateAddress != null && ((Address.IsNonUkAddress && stateAddress.Country != Country.UKCountryCode)
            || (!Address.IsNonUkAddress && stateAddress.Country == Country.UKCountryCode)))
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
            
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var address = new Address()
        {            
            AddressLine1 = Address.AddressLine1 ?? "",
            TownOrCity = Address.TownOrCity ?? "",
            Postcode = Address.Postcode ?? "",
            CountryName = Address.CountryName ?? "",
            Country = Address.Country ?? ""
        };

        ConsortiumDetails.PostalAddress = address;

        SessionContext.Set(Session.ConsortiumKey, ConsortiumDetails);

        return RedirectToPage("ConsortiumEmail");

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
}