using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
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
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_PleaseSelect), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? HasLawRegistered { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasLawRegistered), true, ErrorMessage = nameof(StaticTextResource.Supplier_LegalFormLawRegistered_ErrorMessage))]
    public string? LawRegistered { get; set; }

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

        if (LawRegistered == null)
        {
            HasLawRegistered = null;
        }
        else
        {
            HasLawRegistered = (LawRegistered.Length > 0);
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!HasLawRegistered.GetValueOrDefault())
        {
            LawRegistered = string.Empty;
        }

        var lf = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        lf.LawRegistered = LawRegistered;
        tempDataService.Put(LegalForm.TempDataKey, lf);

        return RedirectToPage("LegalFormFormationDate", new { Id });
    }
}