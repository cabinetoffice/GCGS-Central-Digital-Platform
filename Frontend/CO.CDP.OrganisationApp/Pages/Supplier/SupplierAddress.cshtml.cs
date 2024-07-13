using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Shared;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
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

    public async Task<IActionResult> OnGet()
    {
        try
        {
            SetupAddress(true);
            var composed = await organisationClient.GetComposedOrganisation(Id);

            if ((composed.SupplierInfo.CompletedRegAddress && AddressType == Constants.AddressType.Registered)
                || (composed.SupplierInfo.CompletedPostalAddress && AddressType == Constants.AddressType.Postal))
            {
                var address = composed.Organisation.Addresses.FirstOrDefault(a =>
                    a.Type == AddressType.AsApiClientAddressType() && (Address.IsNonUkAddress ? a.CountryName != Constants.Country.UnitedKingdom : a.CountryName == Constants.Country.UnitedKingdom));

                if (address != null)
                {
                    Address.AddressLine1 = address.StreetAddress;
                    Address.TownOrCity = address.Locality;
                    Address.Postcode = address.PostalCode;
                    Address.Country = address.CountryName;
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
        SetupAddress();
        if (!ModelState.IsValid) return Page();

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);

            ICollection<OrganisationAddress> addresses = [
                            new OrganisationAddress(
                            streetAddress: Address.AddressLine1,
                            streetAddress2: null,
                            postalCode: Address.Postcode,
                            locality: Address.TownOrCity,
                            countryName: Address.Country,
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

    private void SetupAddress(bool reset = false)
    {
        if (reset) Address = new AddressPartialModel { UkOrNonUk = UkOrNonUk };

        Address.Heading = AddressType == Constants.AddressType.Registered ?
            "Enter your organisation's registered address" : "Enter your organisation's UK postal address";

        Address.NonUkAddressLink = $"/organisation/{Id}/supplier-information/{AddressType.ToString().ToLower()}-address/non-uk";
    }
}