using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

using System;
using System.ComponentModel.DataAnnotations;


[AuthenticatedSessionNotRequired]
public class FindATenderFeedback() : PageModel
{
    [BindProperty]
    [DisplayName("Enter feedback")]
    [Required(ErrorMessage = "Enter feedback")]
    public string? Feedback { get; set; }

    [BindProperty]
    [DisplayName("What's it to do with")]
    [Required(ErrorMessage = "Chose a feedback option")]
    public string? FeedbackOption { get; set; }

    [BindProperty]
    [DisplayName("Specific Page")]
    [RequiredIf("FeedbackOption", "SpecificPage", ErrorMessage = "URL of specific page is required")]
    public string? UrlOfPage { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public string? Email { get; set; }


    public IActionResult OnGet()
    {

        return Page();
    }

    public IActionResult OnPost(string? redirectUri = null)
    {
        if ((!string.IsNullOrEmpty(ModelState[nameof(Name)]!.RawValue as string)) && (ModelState[nameof(Email)].RawValue == string.Empty))
        {
            ModelState[nameof(Email)].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid;
            ModelState[nameof(Email)].Errors.Add("Enter an Email address");
            return Page();
        }

        if ((!string.IsNullOrEmpty(ModelState[nameof(Email)]!.RawValue as string)) && (ModelState[nameof(Name)]!.RawValue == string.Empty))
        {
            ModelState[nameof(Name)].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid;
            ModelState[nameof(Name)].Errors.Add("Enter a name");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("YourDetails", new { RedirectUri = Helper.ValidRelativeUri(redirectUri) ? redirectUri : default });
    }
}