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

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            SupplierInformation = await organisationClient.GetOrganisationSupplierInformationAsync(id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }
}