using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

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

    [BindProperty]
    public required List<OperationType>? OperationTypes { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        ComposedOrganisation composed;
        try
        {
            composed = await organisationClient.GetComposedOrganisation(id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        SupplierInformation = composed.SupplierInfo;
        WebsiteAddress = composed.Organisation.ContactPoint.Url?.ToString();
        EmailAddress = composed.Organisation.ContactPoint.Email;

        var vatIdentifier = Helper.GetVatIdentifier(composed.Organisation);
        if (vatIdentifier != null) VatNumber = vatIdentifier.Id;

        var registeredAddrress = composed.Organisation.Addresses.FirstOrDefault(i => i.Type == AddressType.Registered);
        if (registeredAddrress != null) RegisteredAddress = registeredAddrress;

        var postalAddrress = composed.Organisation.Addresses.FirstOrDefault(i => i.Type == AddressType.Postal);
        if (postalAddrress != null) PostalAddress = postalAddrress;

        var operationTypes = composed.SupplierInfo.OperationTypes.ToList();
        if (operationTypes != null) OperationTypes = operationTypes;

        return Page();
    }
}