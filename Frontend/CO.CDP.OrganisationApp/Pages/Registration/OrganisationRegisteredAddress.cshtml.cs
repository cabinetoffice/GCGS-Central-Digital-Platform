using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Shared;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationRegisteredAddressModel(ISession session, ICharityCommissionApi charityCommissionApi, ICompaniesHouseApi companiesHouseApi) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationAddressPage;

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";
    
    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public async Task OnGet()
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

        if ((RegistrationDetails.OrganisationCountryCode == Country.UKCountryCode) &&
            (RegistrationDetails.OrganisationHasCompaniesHouseNumber ?? false) &&
            (string.IsNullOrEmpty(Address.AddressLine1)) &&
            (string.IsNullOrEmpty(Address.TownOrCity)) &&
            (string.IsNullOrEmpty(Address.Postcode)))
        {
            var details = await companiesHouseApi.GetRegisteredAddress(RegistrationDetails.OrganisationIdentificationNumber!);

            if (details != null)
            {
                Address.AddressLine1 = details.AddressLine1;
                Address.TownOrCity = details.Locality;
                Address.Postcode = details.PostalCode;
                Address.Country = RegistrationDetails.OrganisationCountryCode;
            }
        }

        if ((RegistrationDetails.OrganisationCountryCode == Country.UKCountryCode) &&
            (RegistrationDetails.OrganisationScheme == OrganisationSchemeType.CharityCommissionEnglandWales) &&
            (string.IsNullOrEmpty(Address.AddressLine1)) &&
            (string.IsNullOrEmpty(Address.TownOrCity)) &&
            (string.IsNullOrEmpty(Address.Postcode)))
        {
            if (RegistrationDetails.OrganisationIdentificationNumber != null)
            {
                var details = await charityCommissionApi.GetCharityDetails(RegistrationDetails.OrganisationIdentificationNumber);

                if (details != null)
                {
                    Address.AddressLine1 = details.AddressLine2;
                    Address.TownOrCity = details.AddressLine3;
                    Address.Postcode = details.PostalCode;
                    Address.Country = RegistrationDetails.OrganisationCountryCode;
                }
            }
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

        return RedirectToPage("SupplierOrganisationType");
    }

    private void SetupAddress(bool reset = false)
    {
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        if (Address.IsNonUkAddress)
        {
            Address.Heading = StaticTextResource.OrganisationRegistration_Address_NonUk_Heading;
            Address.AddressHint = StaticTextResource.OrganisationRegistration_Address_NonUk_Hint;
        } else
        {
            if(RegistrationDetails.OrganisationType == OrganisationType.Buyer)
            {
                Address.Heading = StaticTextResource.OrganisationRegistration_Address_Buyer_Heading;
                Address.AddressHint = StaticTextResource.OrganisationRegistration_Address_Buyer_Hint;
            } else
            {
                Address.Heading = StaticTextResource.OrganisationRegistration_Address_Supplier_Heading;
                Address.AddressHint = StaticTextResource.OrganisationRegistration_Address_Supplier_Hint;
            }
        }

        Address.NonUkAddressLink = $"/registration/organisation-registered-address/non-uk{(RedirectToSummary == true ? "?frm-summary" : "")}";
    }
}
