using CO.CDP.Common;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class BuyerSelectDevolvedRegulationModel(ISession session) : PageModel
{
    [BindProperty]
    [NotEmpty(ErrorMessage = "Select the do devolved regulations apply to your organisation?")]
    public required List<string> Regulations { get; set; } = [];

    public IActionResult OnGet()
    {
        var registrationDetails = VerifySession();

        Regulations = registrationDetails.Regulations ?? [];

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();
        registrationDetails.Regulations = Regulations;
        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("OrganisationSelection");
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}