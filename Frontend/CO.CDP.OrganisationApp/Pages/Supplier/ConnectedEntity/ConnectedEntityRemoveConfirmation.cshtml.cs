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
IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ConnectedPersonId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_ConfirmRemoveError))]
    public bool? ConfirmRemove { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", true, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_DayRequiredError))]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_DayInvalidError))]
    public string? EndDay { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", true, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_MonthRequiredError))]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_MonthInvalidError))]
    public string? EndMonth { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", true, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_YearRequiredError))]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityRemoveConfirmation_YearInvalidError))]
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConfirmRemove == true)
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
            endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime();
            await organisationClient.DeleteConnectedEntityAsync(Id, ConnectedPersonId, new DeleteConnectedEntity(endDate));
        }

        return RedirectToPage("ConnectedPersonSummary", new { Id });
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
