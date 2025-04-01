using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class ConnectedPersonSummaryModel(
    IFlashMessageService flashMessageService,
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty] public ICollection<ConnectedEntityLookup> ConnectedEntities { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedPersonSummary_AddAnotherConnectedPersonError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? HasConnectedEntity { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        return await InitPage(selected);
    }

    public async Task<IActionResult> OnGetRemove([FromQuery(Name = "entity-id")] Guid entityId)
    {
        if (!await DeleteConnectedEntityAsync(entityId))
        {
            return await InitPage(true);
        }

        return Redirect($"/organisation/{Id}/supplier-information/connected-person/{@entityId}/remove");
    }

    private async Task<IActionResult> InitPage(bool? selected)
    {
        try
        {
            ConnectedEntities = (await organisationClient.GetConnectedEntities(Id))
                .Where(cp => !cp.Deleted)
                .ToList();

            session.Remove(Session.ConnectedPersonKey);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        HasConnectedEntity = selected;

        return Page();
    }

    private async Task<bool> DeleteConnectedEntityAsync(Guid connectedPersonId)
    {
        DeleteConnectedEntityResult result = await organisationClient.DeleteConnectedEntityAsync(Id, connectedPersonId);

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

    public async Task<IActionResult> OnPost()
    {
        ConnectedEntities = await organisationClient.GetConnectedEntities(Id);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage(HasConnectedEntity == true ? "/Supplier/ConnectedEntity/ConnectedEntityDeclaration" : "/Supplier/SupplierInformationSummary", new { Id });
    }
}
