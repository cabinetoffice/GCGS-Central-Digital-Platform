using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityPersonTypeModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the relevant option")]
    public Constants.ConnectedEntityType? ConnectedEntityType { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public bool? SupplierHasCompanyHouseNumber { get; set; }

    public IActionResult OnGet()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierCompanyQuestion", new { Id });
        }

        ConnectedEntityType = state.ConnectedEntityType;
        SupplierHasCompanyHouseNumber = state.SupplierHasCompanyHouseNumber;
        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierCompanyQuestion", new { Id });
        }

        SupplierHasCompanyHouseNumber = state.SupplierHasCompanyHouseNumber;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        state.ConnectedEntityType = ConnectedEntityType;
        session.Set(Session.ConnectedPersonKey, state);

        return RedirectToPage(
            state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
                                            ? "ConnectedEntityOrganisationCategory"
                                            : "ConnectedEntityIndividualCategory"
            , new { Id });
    }
}