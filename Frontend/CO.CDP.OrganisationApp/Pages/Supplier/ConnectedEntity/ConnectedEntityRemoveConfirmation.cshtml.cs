using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityRemoveConfirmationModel(
    IFlashMessageService flashMessageService,
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ConnectedPersonId { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_ConfirmRemoveError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? ConfirmRemove { get; set; }

    [BindProperty]
    public string? EndDate { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var ce = await GetConnectedEntity(organisationClient);
        if (ce == null)
            return Redirect("/page-not-found");

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConfirmRemove == true)
        {
            var ce = await GetConnectedEntity(organisationClient);
            if (ce == null)
                return Redirect("/page-not-found");

            if (!await DeleteConnectedEntityAsync())
            {
                return Page();
            }
        }

        return RedirectToPage("ConnectedPersonSummary", new { Id });
    }

    private async Task<bool> DeleteConnectedEntityAsync()
    {
        DeleteConnectedEntityResult result = await organisationClient.DeleteConnectedEntityAsync(Id, ConnectedPersonId);

        if (!result.Success)
        {
            flashMessageService.SetFlashMessage
            (
                FlashMessageType.Important,
                    heading: StaticTextResource.ErrorMessageList_ConnectedPersons_Cannot_Remove,
                    urlParameters: new() {
                        { "organisationIdentifier", Id.ToString() },
                        { "formId", result.FormGuid.ToString() },
                        { "sectionId", result.SectionGuid.ToString() }
                    }
            );
        }

        return result.Success;
    }

    private async Task<CO.CDP.Organisation.WebApiClient.ConnectedEntityLookup?> GetConnectedEntity(IOrganisationClient organisationClient)
    {
        try
        {
            var connectedEntities = await organisationClient.GetConnectedEntitiesAsync(Id);
            var connectedEntity = connectedEntities.FirstOrDefault(ce => ce.EntityId == ConnectedPersonId);
            return connectedEntity;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}
