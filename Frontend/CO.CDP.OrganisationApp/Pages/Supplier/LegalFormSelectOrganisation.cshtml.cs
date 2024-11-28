using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class LegalFormSelectOrganisationModel(
    ITempDataService tempDataService,
    IOrganisationClient organisationClient) : PageModel
{

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.Global_SelectAnOption), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? RegisteredOrg { get; set; }


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
        RegisteredOrg = lf.RegisteredLegalForm;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ta = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        ta.RegisteredLegalForm = RegisteredOrg;
        tempDataService.Put(LegalForm.TempDataKey, ta);

        return RedirectToPage("LegalFormLawRegistered", new { Id });
    }

    public static Dictionary<string, string> OrganisationLegalForm => new()
    {
        { "LimitedCompany", StaticTextResource.Supplier_LegalFormSelectOrganisation_LimitedCompany}, 
        { "LLP", StaticTextResource.Supplier_LegalFormSelectOrganisation_LLP}, 
        { "LimitedPartnership", StaticTextResource.Supplier_LegalFormSelectOrganisation_LP}, 
        { "Other", StaticTextResource.Supplier_LegalFormSelectOrganisation_Other}
    };
}