using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages;

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
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }
}