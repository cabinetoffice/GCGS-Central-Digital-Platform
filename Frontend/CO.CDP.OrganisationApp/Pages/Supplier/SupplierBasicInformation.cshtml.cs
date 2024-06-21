using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierBasicInformationModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    public SupplierInformation? SupplierInformation { get; set; }

    [BindProperty]
    public string? VatNumber { get; set; }

    [BindProperty]
    public string? WebsiteAddress { get; set; }

    [BindProperty]
    public string? EmailAddress { get; set; }

    [BindProperty]
    public Address? RegisteredAddress { get; set; }

    [BindProperty]
    public Address? PostalAddress { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(id);

            SupplierInformation = composed.SupplierInfo;
            WebsiteAddress = composed.Organisation.ContactPoint.Url?.ToString();
            EmailAddress = composed.Organisation.ContactPoint.Email;

            var vatIdentifier = composed.Organisation.AdditionalIdentifiers.FirstOrDefault(i => i.Scheme == "VAT");
            if (vatIdentifier != null) VatNumber = vatIdentifier.Id;

            var registeredAddrress = composed.Organisation.Addresses.FirstOrDefault(i => i.Type == AddressType.Registered);
            if (registeredAddrress != null) RegisteredAddress = registeredAddrress;

            var postalAddrress = composed.Organisation.Addresses.FirstOrDefault(i => i.Type == AddressType.Postal);
            if (postalAddrress != null) PostalAddress = postalAddrress;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }
}