using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityOrganisationCategoryModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityOrganisationCategory_SelectCategoryError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public ConnectedEntityOrganisationCategoryType? ConnectedEntityCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public bool RegisteredWithCh { get; set; }

    public IActionResult OnGet()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierCompanyQuestion", new { Id });
        }

        ConnectedEntityCategory = state.ConnectedEntityOrganisationCategoryType;
        RegisteredWithCh = state.SupplierHasCompanyHouseNumber ?? false;
        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierCompanyQuestion", new { Id });
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        state.ConnectedEntityOrganisationCategoryType = ConnectedEntityCategory;
        session.Set(Session.ConnectedPersonKey, state);
        return RedirectToPage("ConnectedEntityOrganisationName", new { Id });
    }

    public static Dictionary<string, string> ConnectedEntityCategoryOption => new()
    {
        { "RegisteredCompany", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityOrganisationCategory_Option_RegisteredCompany },
        { "RegisteredCompanyOrEquivalent", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityOrganisationCategory_Option_RegisteredCompanyOrEquivalent },        
        { "Director", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityOrganisationCategory_Option_Director },
        { "ParentOrSubCompany", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityOrganisationCategory_Option_ParentOrSubCompany },
        { "CompanyOverTaken", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityOrganisationCategory_Option_CompanyTakenOver },
        { "OrgWithSignificantControl", StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityOrganisationCategory_Option_OrgWithSignificantControl }
    };
}