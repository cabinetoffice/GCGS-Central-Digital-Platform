using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class LegalFormLawRegisteredModel(
    ITempDataService tempDataService,
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please enter the law under which your organisation is registered")]
    public string? LawRegistered { get; set; }

    [BindProperty]
    public bool? RegisteredUnderAct2006 { get; set; }

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

        LawRegistered = lf.LawRegistered;
        RegisteredUnderAct2006 = lf.RegisteredUnderAct2006;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var lf = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        lf.LawRegistered = LawRegistered;
        tempDataService.Put(LegalForm.TempDataKey, lf);

        return RedirectToPage("LegalFormFormationDate", new { Id });
    }
}