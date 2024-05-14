using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationEmailModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Enter the organisation email address")]
    [Required(ErrorMessage = "Enter the organisation email address")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? EmailAddress { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();

        EmailAddress = registrationDetails.OrganisationEmailAddress;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.OrganisationEmailAddress = EmailAddress;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("OrganisationRegisteredAddress");
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