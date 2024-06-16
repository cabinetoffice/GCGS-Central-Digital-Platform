using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class PrincipalOfficeAddressUkModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{

    public override ISession SessionContext => session;

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
    [DisplayName("Region")]
    [Required(ErrorMessage = "Enter your Region")]
    public string? Region { get; set; }

    [BindProperty]
    [DisplayName("Postcode")]
    [Required(ErrorMessage = "Enter your postcode")]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    [Required(ErrorMessage = "Enter your country")]
    public string? Country { get; set; } = "United Kingdom";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var getOrganisationTask = organisationClient.GetOrganisationAsync(id);
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);

            await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);

            if (getSupplierInfoTask.Result.CompletedRegAddress)
            {
                var hasRegisteredUkAddress = getOrganisationTask.Result.Addresses.FirstOrDefault(i => i.CountryName == "United Kingdom");
                AddressLine1 = hasRegisteredUkAddress?.StreetAddress;
                AddressLine2 = hasRegisteredUkAddress?.StreetAddress2;
                TownOrCity = hasRegisteredUkAddress?.Locality;
                Region = hasRegisteredUkAddress?.Region;
                Postcode = hasRegisteredUkAddress?.PostalCode;
                Country = hasRegisteredUkAddress?.CountryName;
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

            await organisationClient.UpdateOrganisationAsync(Id,
              new UpdatedOrganisation
                (
                    type: OrganisationUpdateType.Address,

                    organisation: new OrganisationInfo(null,
                        addresses: [
                            new OrganisationAddress (
                            streetAddress : AddressLine1,
                            streetAddress2 : AddressLine2,
                            postalCode : Postcode,
                            locality: TownOrCity,
                            countryName: Country,
                            type: AddressType.Registered,
                            region: Region
                            )]
                )));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}