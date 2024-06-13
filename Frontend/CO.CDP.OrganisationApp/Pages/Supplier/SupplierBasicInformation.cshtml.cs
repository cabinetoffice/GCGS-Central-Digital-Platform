using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierBasicInformationModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    public SupplierInformation? SupplierInformation { get; set; }

    [BindProperty]
    public string? VatNumber { get; set; }

    [BindProperty]
    public Address? RegisteredAddress { get; set; }

    [BindProperty]
    public Address? PostalAddress { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var getOrganisationTask = organisationClient.GetOrganisationAsync(id);
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);

            await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);

            SupplierInformation = getSupplierInfoTask.Result;
            var organisation = getOrganisationTask.Result;

            var vatIdentifier = organisation.AdditionalIdentifiers.FirstOrDefault(i => i.Scheme == "VAT");
            if (vatIdentifier != null)
            {
                VatNumber = vatIdentifier.Id;
            }

            var registeredAddrress = organisation.Addresses.FirstOrDefault(i => i.Type == AddressType.Registered);
            if (registeredAddrress != null)
            {
                RegisteredAddress = registeredAddrress;
            }

            var postalAddrress = organisation.Addresses.FirstOrDefault(i => i.Type == AddressType.Postal);
            if (postalAddrress != null)
            {
                PostalAddress = postalAddrress;
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }
}