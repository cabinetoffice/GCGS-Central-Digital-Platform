using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSession]
public class PrivacyPolicyModel(ISession session) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [DisplayName("Yes, I have read and agree to the Central Digital Platform service privacy policy")]
    [Required(ErrorMessage = "Select if you have read and agree to the Central Digital Platform service privacy policy")]
    public bool? AgreeToPrivacy { get; set; }

    public IActionResult OnGet()
    {
        if (UserDetails.PersonId.HasValue)
        {
            return RedirectToPage("OrganisationSelection");
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("YourDetails");
    }
}