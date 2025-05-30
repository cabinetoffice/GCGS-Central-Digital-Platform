using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Shared;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class SupplierAddressModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Constants.AddressType AddressType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    [BindProperty]
    public AddressPartialModel Address { get; set; } = new();

    public SupplierType? SupplierType { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);
            SupplierType = composed.SupplierInfo.SupplierType;
            SetupAddress(true);

            if ((composed.SupplierInfo.CompletedRegAddress && AddressType == Constants.AddressType.Registered)
                || (composed.SupplierInfo.CompletedPostalAddress && AddressType == Constants.AddressType.Postal))
            {
                var address = composed.Organisation.Addresses.FirstOrDefault(a =>
                    a.Type == AddressType.AsApiClientAddressType() && (Address.IsNonUkAddress ? a.Country != Country.UKCountryCode : a.Country == Country.UKCountryCode));

                if (address != null)
                {
                    Address.AddressLine1 = address.StreetAddress;
                    Address.TownOrCity = address.Locality;
                    Address.Postcode = address.PostalCode;
                    Address.Country = address.Country;
                }
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            SupplierType = supplierInfo.SupplierType;

            SetupAddress(false);
            if (!ModelState.IsValid)
                return Page();

            ICollection<OrganisationAddress> addresses = [
                            new OrganisationAddress(
                            streetAddress: Address.AddressLine1,
                            postalCode: Address.Postcode,
                            locality: Address.TownOrCity,
                            countryName: Address.CountryName,
                            country: Address.Country,
                            type: AddressType.AsApiClientAddressType(),
                            region: null)];

            await organisationClient.UpdateOrganisationAddresses(Id, addresses);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }

    private void SetupAddress(bool reset)
    {
        if (reset)
            Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        Address.Heading = AddressType == Constants.AddressType.Registered ?
            StaticTextResource.Supplier_Address_EnterRegisteredAddress : StaticTextResource.Supplier_Address_EnterOrganisationPostalAddress;
        if (AddressType == Constants.AddressType.Registered)
        {
            if (Address.IsNonUkAddress)
            {
                Address.AddressHint = StaticTextResource.Supplier_Address_EnterOrganisationRegistered_NonUk_Hint;
            }
            else
            {
                Address.AddressHint = StaticTextResource.Supplier_Address_EnterOrganisationRegistered_Hint;
            }
        }

        if (AddressType == Constants.AddressType.Postal && SupplierType == CDP.Organisation.WebApiClient.SupplierType.Individual)
        {
            Address.Heading = StaticTextResource.Supplier_Address_EnterIndividualPostalAddress;
            Address.AddressHint = StaticTextResource.Supplier_Address_EnterIndividualPostalAddress_Hint;
        }

        Address.NonUkAddressLink = $"/organisation/{Id}/supplier-information/{AddressType.ToString().ToLower()}-address/non-uk";
    }
}