using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityOrganisationCategoryModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the category that best describes the 'connected person'")]
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
        if (state == null || !ModelState.IsValid)
        {
            return Page();
        }

        state.ConnectedEntityOrganisationCategoryType = ConnectedEntityCategory;
        session.Set(Session.ConnectedPersonKey, state);
        return RedirectToPage("ConnectedEntityOrganisationName", new { Id });
    }

    public static Dictionary<string, string> ConnectedEntityCategoryOption => new()
    {
        { "RegisteredCompany", "registered company" },
        { "Director", "director or organisation with the same responsibilities"},
        { "ParentOrSubCompany", "parent or subsidiary company"},
        { "CompanyOverTaken", "a company your organisation has taken over"},
        { "OrgWithSignificantControl", "any other organisation with significant influence or control"}
    };
}