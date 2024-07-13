using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntitySelectTypeModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public Constants.ConnectedEntityType? ConnectedEntityType { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public IActionResult OnGet()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntityQuestion", new { Id });
        }

        ConnectedEntityType = state.ConnectedEntityType;
        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null || !ModelState.IsValid)
        {
            return Page();
        }

        state.ConnectedEntityType = ConnectedEntityType;
        session.Set(Session.ConnectedPersonKey, state);

        return RedirectToPage("ConnectedEntitySelectCategory", new { Id });
    }
}