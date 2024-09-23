using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntitySupplierHasControlModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select if your organisation influenced or controlled by another person or company")]
    public bool? ControlledByPersonOrCompany { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }


    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {
            await organisationClient.GetOrganisationAsync(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        var cp = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey) ?? new ConnectedEntityState { SupplierOrganisationId = Id };
        session.Set(Session.ConnectedPersonKey, cp);

        if (selected.HasValue)
        {
            ControlledByPersonOrCompany = selected.Value;
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var cp = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (cp == null || !ModelState.IsValid)
        {
            return Page();
        }

        if (ControlledByPersonOrCompany == true)
        {
            return RedirectToPage("ConnectedEntitySupplierCompanyQuestion", new { Id });
        }
        else
        {
            try
            {
                var connectedEntity = await organisationClient.GetConnectedEntitiesAsync(Id);

                if (connectedEntity.Count == 0)
                    await organisationClient.UpdateSupplierCompletedConnectedPerson(Id);

                session.Remove(Session.ConnectedPersonKey);
                return RedirectToPage("/Supplier/SupplierInformationSummary", new { Id });
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return Redirect("/page-not-found");
            }
        }
    }
}