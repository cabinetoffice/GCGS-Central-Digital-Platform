using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class AddressPostalSameAsRegisteredQuestionModel
    (IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? SameAsRegiseterdAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        if (selected == false)
        {
            SameAsRegiseterdAddress = false;
        }
        else
        {
            ComposedOrganisation composed;
            try
            {
                composed = await organisationClient.GetComposedOrganisation(Id);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return Redirect("/page-not-found");
            }

            if (composed.SupplierInfo.CompletedRegAddress && composed.SupplierInfo.CompletedPostalAddress)
            {
                var registeredAddress = composed.Organisation.Addresses.FirstOrDefault(a => a.Type == AddressType.Registered);
                var postalAddress = composed.Organisation.Addresses.FirstOrDefault(a => a.Type == AddressType.Postal);
                SameAsRegiseterdAddress = AreSameAddress(registeredAddress, postalAddress);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (SameAsRegiseterdAddress == true)
        {
            try
            {
                var organisation = await organisationClient.GetOrganisationAsync(Id);
                var registeredAddress = organisation.Addresses.FirstOrDefault(a => a.Type == AddressType.Registered);
                if (registeredAddress != null)
                {
                    ICollection<OrganisationAddress> addresses = [
                                new OrganisationAddress(
                                    streetAddress: registeredAddress.StreetAddress,
                                    streetAddress2: registeredAddress.StreetAddress2,
                                    postalCode: registeredAddress.PostalCode,
                                    locality: registeredAddress.Locality,
                                    countryName: registeredAddress.CountryName,
                                    type: AddressType.Postal,
                                    region: registeredAddress.Region)];

                    await organisationClient.UpdateOrganisationAddresses(Id, addresses);
                }
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return Redirect("/page-not-found");
            }

            return RedirectToPage("SupplierBasicInformation", new { Id });
        }

        return RedirectToPage("AddressTypeQuestion", new { Id, AddressType = Constants.AddressType.Postal.ToString().ToLower() });
    }

    private static bool AreSameAddress(Address? registeredAddress, Address? postalAddress)
    {
        return registeredAddress != null
                && postalAddress != null
                && registeredAddress.StreetAddress == postalAddress.StreetAddress
                && registeredAddress.StreetAddress2 == postalAddress.StreetAddress2
                && registeredAddress.Locality == postalAddress.Locality
                && registeredAddress.Region == postalAddress.Region
                && registeredAddress.PostalCode == postalAddress.PostalCode
                && registeredAddress.CountryName == postalAddress.CountryName;
    }
}