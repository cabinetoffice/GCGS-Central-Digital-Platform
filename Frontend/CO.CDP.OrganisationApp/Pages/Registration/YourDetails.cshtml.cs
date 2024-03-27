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
    [RegularExpression(RegExPatterns.EmailAddress, ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? Email { get; set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);
        registrationDetails ??= new RegistrationDetails();
        registrationDetails.FirstName = FirstName;
        registrationDetails.LastName = LastName;
        registrationDetails.Email = Email;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("OrganisationDetails");
    }
}
