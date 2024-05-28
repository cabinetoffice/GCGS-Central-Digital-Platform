using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class BuyerDevolvedRegulationModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the do devolved regulations apply to your organisation?")]
    public string? Devolved { get; set; }

    public IActionResult OnGet()
    {
        var registrationDetails = VerifySession();

        Devolved = registrationDetails.Devolved;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();
        registrationDetails.Devolved = Devolved;        

        if (Devolved == "yes")
        {
            session.Set(Session.RegistrationDetailsKey, registrationDetails);
            return RedirectToPage("BuyerSelectDevolvedRegulation");
        }
        else
        {
            registrationDetails.Regulations = [];
            session.Set(Session.RegistrationDetailsKey, registrationDetails);
            return RedirectToPage("OrganisationSelection");
        }
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}