using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationNameModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Enter the organisation name")]
    [Required(ErrorMessage = "Enter the organisation name")]
    public string? OrganisationName { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();

        OrganisationName = registrationDetails.OrganisationName;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.OrganisationName = OrganisationName;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("OrganisationEmail");
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);
        if (registrationDetails == null)
        {
            //show error page (Once we finalise)
            throw new Exception("Shoudn't be here");
        }
        return registrationDetails;
    }
}