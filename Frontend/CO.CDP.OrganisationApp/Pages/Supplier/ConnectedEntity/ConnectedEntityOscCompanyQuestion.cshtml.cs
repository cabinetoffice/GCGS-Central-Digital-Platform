using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityOscCompanyQuestionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasOscCompaniesHouseNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasOscCompaniesHouseNumber), true, ErrorMessage = "Please enter the Company registration number.")]
    public string? OscCompaniesHouseNumber { get; set; }
    public string? Caption { get; set; }
    public string? Heading { get; set; }
    public string? Hint { get; set; }

    public IActionResult OnGet(bool? selected)
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModal(state);

        HasOscCompaniesHouseNumber = selected.HasValue ? selected : state.HasOscCompaniesHouseNumber;
        OscCompaniesHouseNumber = state.OscCompaniesHouseNumber;

        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModal(state);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        state.HasOscCompaniesHouseNumber = HasOscCompaniesHouseNumber;
        state.OscCompaniesHouseNumber = OscCompaniesHouseNumber;

        session.Set(Session.ConnectedPersonKey, state);

        return RedirectToPage("ConnectedEntityControlCondition", new { Id, ConnectedEntityId });
    }

    private void InitModal(ConnectedEntityState state)
    {
        Caption = state.GetCaption();
        Heading = $"Is {state.OrganisationName} registered with an overseas equivalent to Companies House?";
        Hint = "Is the 'connected person' registered with a similar non-UK body that incorporates companies?";
    }
}
