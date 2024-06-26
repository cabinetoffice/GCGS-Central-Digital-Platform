using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierQualificationCheckAnswerModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }
    [BindProperty]
    public Qualification? Qualification { get; set; }

    public IActionResult OnGet()
    {
        Qualification = tempDataService.PeekOrDefault<Qualification>(Qualification.TempDataKey);
        if (!Validate(Qualification))
        {
            return RedirectToPage("SupplierQualificationAwardingBody", new { Id });
        }
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var qa = tempDataService.GetOrDefault<Qualification>(Qualification.TempDataKey);
        if (!Validate(qa))
        {
            return RedirectToPage("SupplierQualificationAwardingBody", new { Id });
        }

        await organisationClient.UpdateSupplierQualification(Id,
            new Organisation.WebApiClient.Qualification(
                id: qa.Id,
                awardedByPersonOrBodyName: qa.AwardedByPersonOrBodyName,
                name: qa.Name,
                dateAwarded: qa.DateAwarded!.Value));

        tempDataService.Remove(Qualification.TempDataKey);

        return RedirectToPage("SupplierQualificationSummary", new { Id });
    }

    private static bool Validate(Qualification qualification)
    {
        return !string.IsNullOrEmpty(qualification.AwardedByPersonOrBodyName)
            && !string.IsNullOrEmpty(qualification.Name)
            && qualification.DateAwarded.HasValue;
    }
}