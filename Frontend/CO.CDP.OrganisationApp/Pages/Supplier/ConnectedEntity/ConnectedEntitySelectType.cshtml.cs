using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntitySelectTypeModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public Constants.ConnectedEntityType? ConnectedEntityType { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    public async Task<IActionResult> OnGet()
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

            ConnectedEntityType = cp.ConnectedEntityType;
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
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);

            var cp = session.Get<ConnectedPerson>(Session.ConnectedPersonKey) ?? new ConnectedPerson();

            if (ConnectedEntityId.HasValue == true && cp.ConnectedEntityId != ConnectedEntityId)
            {
                session.Remove(Session.ConnectedPersonKey);
                return RedirectToPage("ConnectedQuestion", new { Id });
            }

            cp.ConnectedEntityType = ConnectedEntityType;

            return RedirectToPage("ConnectedEntitySelectCategory", new { Id });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}