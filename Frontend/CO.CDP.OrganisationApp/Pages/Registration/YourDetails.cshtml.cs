using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class YourDetailsModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("First name")]
    [Required(ErrorMessage = "Enter your first name")]
    public string? FirstName { get; set; }

    [BindProperty]
    [DisplayName("Last name")]
    [Required(ErrorMessage = "Enter your last name")]
    public string? LastName { get; set; }

    [BindProperty]
    [DisplayName("Email address")]
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? Email { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        var registrationDetails = VerifySession();

        FirstName = registrationDetails.FirstName;
        LastName = registrationDetails.LastName;
        Email = registrationDetails.Email;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.FirstName = FirstName;
        registrationDetails.LastName = LastName;
        registrationDetails.Email = Email;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationDetails");
        }
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception("Shoudn't be here"); // show error page?
 
        return registrationDetails;
    }
}
