using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntitySignificantPersonWhereRegisteredModel(ISession session) : PageModel
{
    private const string OPTION_COMPANIES_HOUSE = "Companies House";
    private const string OPTION_OTHER = "other";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select an option")]
    public string? RegisterName { get; set; }

    [BindProperty]
    public string? RegisterNameInput { get; set; }

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }
    public string? Heading { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswersOrganisation" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state, true);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswersOrganisation" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);

        if (!ModelState.IsValid) return Page();

        state.RegisterName = RegisterName;

        if (RegisterName == OPTION_OTHER)
        {
            state.RegisterName = RegisterNameInput;
        }

        session.Set(Session.ConnectedPersonKey, state);

        return RedirectToPage("ConnectedEntityCheckAnswersOrganisation", new { Id, ConnectedEntityId });
    }

    private void InitModal(ConnectedEntityState state, bool reset = false)
    {
        Heading = $"Select where {state.OrganisationName} is registered as person with significant control";

        if (reset)
        {
            RegisterName = state.RegisterName;

            if (RegisterName != OPTION_COMPANIES_HOUSE)
            {
                RegisterName = OPTION_OTHER;
                RegisterNameInput = state.RegisterName;
            }
        }
    }

    private (bool valid, ConnectedEntityState state) ValidatePage()
    {
        var cp = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (cp == null ||
            cp.SupplierOrganisationId != Id ||
            (ConnectedEntityId.HasValue && cp.ConnectedEntityId.HasValue && cp.ConnectedEntityId != ConnectedEntityId))
        {
            return (false, new());
        }
        return (true, cp);
    }
}