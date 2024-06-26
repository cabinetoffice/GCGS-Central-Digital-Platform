using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class AddressUkModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [DisplayName("Address line 1")]
    [Required(ErrorMessage = "Enter your address line 1")]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [DisplayName("Address line 2 (optional)")]
    public string? AddressLine2 { get; set; }

    [BindProperty]
    [DisplayName("Town or city")]
    [Required(ErrorMessage = "Enter your town or city")]
    public string? TownOrCity { get; set; }

    [BindProperty]
    [DisplayName("Postcode")]
    [Required(ErrorMessage = "Enter your postcode")]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    [Required(ErrorMessage = "Enter your country")]
    public string? Country { get; set; } = Constants.Country.UnitedKingdom;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Constants.AddressType AddressType { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);

            if ((composed.SupplierInfo.CompletedRegAddress && AddressType == Constants.AddressType.Registered)
                || (composed.SupplierInfo.CompletedPostalAddress && AddressType == Constants.AddressType.Postal))
            {
                var address = composed.Organisation.Addresses.FirstOrDefault(a =>
                    a.Type == AddressType.AsApiClientAddressType() && a.CountryName == Constants.Country.UnitedKingdom);

                if (address != null)
                {
                    AddressLine1 = address.StreetAddress;
                    AddressLine2 = address.StreetAddress2;
                    TownOrCity = address.Locality;
                    Postcode = address.PostalCode;
                    Country = address.CountryName;
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);

            ICollection<OrganisationAddress> addresses = [
                            new OrganisationAddress(
                            streetAddress: AddressLine1,
                            streetAddress2: AddressLine2,
                            postalCode: Postcode,
                            locality: TownOrCity,
                            countryName: Country,
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
}