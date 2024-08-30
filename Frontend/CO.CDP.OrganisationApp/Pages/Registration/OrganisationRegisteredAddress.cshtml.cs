using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationRegisteredAddressModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationAddressPage;

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
        SetupAddress(true);

        if ((Address.IsNonUkAddress && RegistrationDetails.OrganisationCountryCode != Country.UKCountryCode)
            || (!Address.IsNonUkAddress && RegistrationDetails.OrganisationCountryCode == Country.UKCountryCode))
        {
            Address.AddressLine1 = RegistrationDetails.OrganisationAddressLine1;
            Address.TownOrCity = RegistrationDetails.OrganisationCityOrTown;
            Address.Postcode = RegistrationDetails.OrganisationPostcode;
            Address.Country = RegistrationDetails.OrganisationCountryCode;
        }
    }

    public IActionResult OnPost()
    {
        SetupAddress();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationAddressLine1 = Address.AddressLine1;
        RegistrationDetails.OrganisationCityOrTown = Address.TownOrCity;
        RegistrationDetails.OrganisationPostcode = Address.Postcode;
        RegistrationDetails.OrganisationRegion = null;
        RegistrationDetails.OrganisationCountryName = Address.CountryName ?? RegistrationDetails.OrganisationCountryName;
        RegistrationDetails.OrganisationCountryCode = Address.Country ?? RegistrationDetails.OrganisationCountryCode;

        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }

        if (RegistrationDetails.OrganisationType == OrganisationType.Buyer)
        {
            return RedirectToPage("BuyerOrganisationType");
        }

        return RedirectToPage("OrganisationDetailsSummary");
    }

    private void SetupAddress(bool reset = false)
    {
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        Address.Heading = Address.IsNonUkAddress ?
            "Enter the organisation's registered non-UK address" : "Enter the organisation's registered UK address";

        Address.AddressHint = "The address of the company or organisation which is recorded on public records or within the public domain. This will be displayed on notices.";

        Address.NonUkAddressLink = $"/registration/organisation-registered-address/non-uk{(RedirectToSummary == true ? "?frm-summary" : "")}";
    }
}