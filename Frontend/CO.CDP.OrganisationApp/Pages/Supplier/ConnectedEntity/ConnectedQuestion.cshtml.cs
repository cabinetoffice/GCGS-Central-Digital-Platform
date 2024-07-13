using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedQuestionModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select if your organisation influenced or controlled by another person or company")]
    public bool? ControlledByPersonOrCompany { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }


    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {            
            var cp = session.Get<ConnectedPerson>(Session.ConnectedPersonKey) ?? new ConnectedPerson();

            if (ConnectedEntityId.HasValue == true && cp.ConnectedEntityId != ConnectedEntityId)
            {
                var connectedEnity = await organisationClient.GetConnectedEntityAsync(Id, ConnectedEntityId.Value);
                
                cp.SupplierInformationOrganisationId = Id;
                cp.SupplierHasCompanyHouseNumber = cp.SupplierHasCompanyHouseNumber;
                cp.ConnectedEntityId = ConnectedEntityId;
            }
            else
            {
                await organisationClient.GetOrganisationAsync(Id);
            }

            session.Set(Session.ConnectedPersonKey, cp);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        if (selected.HasValue)
        {
            ControlledByPersonOrCompany = selected.Value;
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

            if (ControlledByPersonOrCompany == true)
            {
                return RedirectToPage("ConnectedCompaniesQuestion", new { Id });
            }
            else
            {
                var connectedEntity = await organisationClient.GetConnectedEntitiesAsync(Id);

                if (connectedEntity.Count == 0)
                    await organisationClient.UpdateSupplierCompletedConnectedPerson(Id);

                session.Remove(Session.ConnectedPersonKey);
                return RedirectToPage("/Supplier/SupplierInformationSummary", new { Id });
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}

public class ConnectedPerson
{
    public Guid? SupplierInformationOrganisationId { get; set; }
    public Guid? ConnectedEntityId { get; set; }
    public bool? SupplierHasCompanyHouseNumber { get; set; }
    public Constants.ConnectedEntityType? ConnectedEntityType { get; set; }
}