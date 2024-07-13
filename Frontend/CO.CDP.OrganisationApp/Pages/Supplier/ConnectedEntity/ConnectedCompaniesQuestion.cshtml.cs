using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedCompaniesQuestionModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? RegisteredWithCh { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {
            var getSupplierInfoTask = await organisationClient.GetOrganisationSupplierInformationAsync(Id);

            var cp = session.Get<ConnectedPerson>(Session.ConnectedPersonKey) ?? new ConnectedPerson();

            if (ConnectedEntityId.HasValue == true && cp.ConnectedEntityId != ConnectedEntityId)
            {
                var connectedEnity = await organisationClient.GetConnectedEntityAsync(Id, ConnectedEntityId.Value);

                return RedirectToPage("ConnectedQuestion", new { Id });
            }
            else
            {
                await organisationClient.GetOrganisationAsync(Id);
            }

            RegisteredWithCh = cp.SupplierHasCompanyHouseNumber;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        if (selected.HasValue)
        {
            RegisteredWithCh = selected.Value;
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
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);

            var cp = session.Get<ConnectedPerson>(Session.ConnectedPersonKey) ?? new ConnectedPerson();

            if (ConnectedEntityId.HasValue == true && cp.ConnectedEntityId != ConnectedEntityId)
            {
                session.Remove(Session.ConnectedPersonKey);
                return RedirectToPage("ConnectedQuestion", new { Id });
            }

            cp.SupplierHasCompanyHouseNumber = (RegisteredWithCh ?? false);

            return RedirectToPage("ConnectedEntitySelectType", new { Id });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}