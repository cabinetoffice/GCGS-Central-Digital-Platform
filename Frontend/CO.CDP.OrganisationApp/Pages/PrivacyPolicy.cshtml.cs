using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

[Authorize]
public class PrivacyPolicyModel : PageModel
{
    [BindProperty]
    [DisplayName("Yes, I have read and agree to the Central Digital Platform service privacy policy")]
    [Required(ErrorMessage = "Select if you have read and agree to the Central Digital Platform service privacy policy")]
    public bool AgreeToPrivacy { get; set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("Registration/YourDetails");
    }
}