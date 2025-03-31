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
public class LegalFormSelectOrganisationModel(
    ITempDataService tempDataService,
    IOrganisationClient organisationClient) : PageModel
{

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_SelectAnOption), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? RegisteredOrg { get; set; }

    public bool? RegisteredUnderAct2006 { get; set; }

    [BindProperty]
    [RequiredIf("RegisteredOrg", "Other", ErrorMessage = nameof(StaticTextResource.Supplier_LegalFormSelectOrganisation_Other_Error))]
    public string? OtherLegalForm { get; set; }

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
        RegisteredUnderAct2006 = lf.RegisteredUnderAct2006;

        if ((RegisteredOrg != null) && (!OrganisationLegalForm.ContainsKey(RegisteredOrg)))
        {
            OtherLegalForm = lf.RegisteredLegalForm;
            RegisteredOrg = "Other";
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (RegisteredOrg != "Other")
        {
            OtherLegalForm = string.Empty;
        }

        var redirectPage = string.Empty;
        var ta = tempDataService.PeekOrDefault<LegalForm>(LegalForm.TempDataKey);
        ta.RegisteredLegalForm = RegisteredOrg;

        if ((string.IsNullOrEmpty(OtherLegalForm)) && (OrganisationLegalForm.ContainsKey(RegisteredOrg!)))
        {
            ta.LawRegistered = "Companies Act 2006";
            redirectPage = "LegalFormFormationDate";
        }
        else
        {
            ta.RegisteredLegalForm = OtherLegalForm;
            ta.LawRegistered = (ta.LawRegistered != null && ta.LawRegistered != "Companies Act 2006") ? ta.LawRegistered: null;
            redirectPage = "LegalFormLawRegistered";
        }
        
        tempDataService.Put(LegalForm.TempDataKey, ta);

        return RedirectToPage(redirectPage, new { Id });
    }

    public static Dictionary<string, string> OrganisationLegalForm => new()
    {
        { "Partnership", StaticTextResource.Supplier_LegalFormSelectOrganisation_Partnership},
        { "LimitedPartnership", StaticTextResource.Supplier_LegalFormSelectOrganisation_LP},
        { "LLP", StaticTextResource.Supplier_LegalFormSelectOrganisation_LLP},
        { "LimitedCompany", StaticTextResource.Supplier_LegalFormSelectOrganisation_LimitedCompany},
        { "PLC", StaticTextResource.Supplier_LegalFormSelectOrganisation_PublicLimitedCompany},
        { "CIC", StaticTextResource.Supplier_LegalFormSelectOrganisation_CIC},
        { "CIO", StaticTextResource.Supplier_LegalFormSelectOrganisation_CIO},
        { "IndustrialProvidentSociety", StaticTextResource.Supplier_LegalFormSelectOrganisation_IndustrialProvidentSoceity},
        { "FinancialMutual", StaticTextResource.Supplier_LegalFormSelectOrganisation_FinancialMutual},
        { "Other", StaticTextResource.Supplier_LegalFormSelectOrganisation_Other}
    };
}