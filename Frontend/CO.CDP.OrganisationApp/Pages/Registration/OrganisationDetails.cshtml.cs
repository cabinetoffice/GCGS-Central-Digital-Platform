using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using CO.CDP.OrganisationApp.Models;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Pages.Registration;


public class OrganisationDetailModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Organisation name")]
    [Required(ErrorMessage = "Enter your organisation name")]
    public string? OrganisationName { get; set; }

    //[BindProperty]
    //[DisplayName("Organisation type")]
    //[Required(ErrorMessage = "Enter your organisation type")]
    //public string? OrganisationType { get; set; }

    [BindProperty]
    [DisplayName("Email address")]
    [Required(ErrorMessage = "Enter your email address")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? EmailAddress { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();

        OrganisationName = registrationDetails.OrganisationName;
        EmailAddress = registrationDetails.OrganisationEmailAddress;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.OrganisationName = OrganisationName;
        registrationDetails.OrganisationEmailAddress = EmailAddress;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("OrganisationIdentification");
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

