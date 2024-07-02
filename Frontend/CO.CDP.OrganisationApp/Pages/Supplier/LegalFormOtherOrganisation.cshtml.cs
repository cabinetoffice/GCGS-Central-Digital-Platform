using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class LegalFormOtherOrganisationModel(
        ITempDataService tempDataService,
        IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select your organisation's registered legal form")]
    public string? OtherOrganisation { get; set; }


    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            await organisationClient.GetOrganisationAsync(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        var lf = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);

        OtherOrganisation = lf.RegisteredLegalForm;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ta = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        ta.RegisteredLegalForm = OtherOrganisation;
        tempDataService.Put(LegalForm.TempDataKey, ta);

        return RedirectToPage("LegalFormLawRegistered", new { Id });
    }
}