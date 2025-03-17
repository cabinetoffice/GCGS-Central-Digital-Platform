using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ProblemDetails = CO.CDP.Organisation.WebApiClient.ProblemDetails;

namespace CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;

public enum RemoveConnectedPersonReason
{
    None,
    NoLongerConnected,
    AddedInError,
    NotRemoved
}

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
    public RemoveConnectedPersonReason ConfirmRemove { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", RemoveConnectedPersonReason.NoLongerConnected, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_DayRequiredError), ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Day, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_DayInvalidError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EndDay { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", RemoveConnectedPersonReason.NoLongerConnected, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_MonthRequiredError), ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Month, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_MonthInvalidError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EndMonth { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", RemoveConnectedPersonReason.NoLongerConnected, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_YearRequiredError), ErrorMessageResourceType = typeof(StaticTextResource))]
    [RegularExpression(RegExPatterns.Year, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_YearInvalidError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EndYear { get; set; }

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
        bool result = true;

        if (!ModelState.IsValid)
        {
            return Page();
        } 

        if (ConfirmRemove == RemoveConnectedPersonReason.NoLongerConnected)
        {
            var ce = await GetConnectedEntity(organisationClient);
            if (ce == null)
                return Redirect("/page-not-found");

            var dateString = $"{EndYear}-{EndMonth!.PadLeft(2, '0')}-{EndDay!.PadLeft(2, '0')}";

            if (!DateTime.TryParse(dateString, out var endDate))
            {
                ModelState.AddModelError(nameof(EndDate), StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_DateInvalidError);
                return Page();
            }

            if (endDate >= DateTime.Today)
            {
                ModelState.AddModelError(nameof(EndDate), StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_DateNotInPastError);
                return Page();
            }

            endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime();
            result = await DeleteConnectedEntityAsync(new DeleteConnectedEntity(endDate));

        }
        else if (ConfirmRemove == RemoveConnectedPersonReason.AddedInError)
        {
            result = await DeleteConnectedEntityAsync(new DeleteConnectedEntity(DateTime.Now.ToUniversalTime()));
        }

        if (!result)
        { 
            return Page();
        }

        return RedirectToPage("ConnectedPersonSummary", new { Id });
    }

    private async Task<bool> DeleteConnectedEntityAsync(DeleteConnectedEntity entity)
    {
        DeleteConnectedEntityResult result = await organisationClient.DeleteConnectedEntityAsync(Id, ConnectedPersonId, entity);

        flashMessageService.SetFlashMessage
        (
            FlashMessageType.Important,
            heading: string.Format(StaticTextResource.ErrorMessageList_ConnectedPersons_Cannot_Remove) //,
            //urlParameters: new() { { "orgId", Id.ToString() }, { "formId", result.FormGuid.ToString() }, { "sectionId", result.SectionGuid.ToString() } }
        );

        return result.Success;
    }

    private async Task<ConnectedEntityLookup?> GetConnectedEntity(IOrganisationClient organisationClient)
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