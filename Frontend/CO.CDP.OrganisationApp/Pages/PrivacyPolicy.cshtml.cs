using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

public class PrivacyPolicyModel(ISession session) : LoggedInUserAwareModel(session)
{
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

    public IActionResult OnPost(string? redirectUri = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("YourDetails", new { RedirectUri = Helper.ValidRelativeUri(redirectUri) ? redirectUri : default });
    }
}