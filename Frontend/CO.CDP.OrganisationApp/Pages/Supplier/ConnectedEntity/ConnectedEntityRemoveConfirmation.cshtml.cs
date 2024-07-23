using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityRemoveConfirmationModel(
IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ConnectedPersonId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please confirm remove connected person option")]
    public bool? ConfirmRemove { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", true, ErrorMessage = "Date of removal must include a day")]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = "Day must be a valid number")]
    public string? EndDay { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", true, ErrorMessage = "Date of removal must include a month")]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = "Month must be a valid number")]
    public string? EndMonth { get; set; }

    [BindProperty]
    [RequiredIf("ConfirmRemove", true, ErrorMessage = "Date of removal must include a year")]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = "Year must be a valid number")]
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
            if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                ModelState.AddModelError(nameof(EndDate), "Date of removal must be a real date");
                return Page();
            }

            await organisationClient.DeleteConnectedEntityAsync(Id, ConnectedPersonId, new DeleteConnectedEntity(endDate));
        }

        return RedirectToPage("ConnectedPersonSummary", new { Id });
    }

    private async Task<Organisation.WebApiClient.ConnectedEntityLookup?> GetConnectedEntity(IOrganisationClient organisationClient)
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