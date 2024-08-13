using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntitySupplierCompanyQuestionModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select yes if supplier organisation registered with Companies House")]
    public bool? RegisteredWithCh { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public IActionResult OnGet(bool? selected)
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        RegisteredWithCh = selected.HasValue ? selected : state.SupplierHasCompanyHouseNumber;
        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        state.SupplierHasCompanyHouseNumber = RegisteredWithCh ?? false;
        session.Set(Session.ConnectedPersonKey, state);

        return RedirectToPage("ConnectedEntityPersonType", new { Id });
    }
}