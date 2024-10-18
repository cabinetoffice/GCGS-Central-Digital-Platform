using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace CO.CDP.OrganisationApp.Pages;

using CO.CDP.Organisation.WebApiClient;
using System.ComponentModel.DataAnnotations;


[AuthenticatedSessionNotRequired]
public class FindATenderFeedback(IOrganisationClient organisationClient) : PageModel
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

    public async Task<IActionResult> OnPost(string? redirectUri = null)
    {
        if ((!string.IsNullOrEmpty(ModelState[nameof(Name)]!.RawValue as string)) &&
            (ModelState[nameof(Email)]!.RawValue as string == string.Empty))
        {
            ModelState[nameof(Email)]!.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid;
            ModelState[nameof(Email)]!.Errors.Add("Enter an Email address");
            return Page();
        }

        if ((!string.IsNullOrEmpty(ModelState[nameof(Email)]!.RawValue as string)) &&
            (ModelState[nameof(Name)]!.RawValue as string == string.Empty))
        {
            ModelState[nameof(Name)]!.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid;
            ModelState[nameof(Name)]!.Errors.Add("Enter a name");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var feedback = new ProvideFeedback(Email ?? string.Empty, Feedback, FeedbackOption, Name ?? string.Empty, UrlOfPage ?? string.Empty);
        var success = await organisationClient.ProvideFeedbackAsync(feedback);

        return RedirectToPage("YourDetails", new { RedirectUri = Helper.ValidRelativeUri(redirectUri) ? redirectUri : default });
    }
}