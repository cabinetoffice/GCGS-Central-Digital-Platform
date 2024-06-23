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

        var ta = tempDataService.GetOrDefault<Qualification>(Qualification.TempDataKey);
        if (!Validate(ta))
        {
            return RedirectToPage("SupplierQualificationAwardingBody", new { Id });
        }

        await organisationClient.UpdateSupplierQualification(Id,
            new Organisation.WebApiClient.Qualification(
                id: ta.Id,
                awardedByPersonOrBodyName: ta.AwardedByPersonOrBodyName,
                name: ta.Name,
                dateAwarded: ta.DateAwarded!.Value));

        tempDataService.Remove(Qualification.TempDataKey);

        return RedirectToPage("QualificationSummary", new { Id });
    }

    private static bool Validate(Qualification qualification)
    {
        return !string.IsNullOrEmpty(qualification.AwardedByPersonOrBodyName)
            && !string.IsNullOrEmpty(qualification.Name)
            && qualification.DateAwarded.HasValue;
    }
}
