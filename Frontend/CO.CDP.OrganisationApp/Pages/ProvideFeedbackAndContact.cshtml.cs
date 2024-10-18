using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace CO.CDP.OrganisationApp.Pages;

using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using System.ComponentModel.DataAnnotations;


[AuthenticatedSessionNotRequired]
public class ProvideFeedbackAndContact(IOrganisationClient organisationClient) : PageModel
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

        var context = Request.Query["context"].ToString().ToUpper();
        if (context != "FEEDBACK" && context != "SUPPORT")
        {
            throw new Exception("Unknown context");
        }

        var feedback = new CO.CDP.Organisation.WebApiClient.ProvideFeedbackAndContact(Email ?? string.Empty, Feedback, FeedbackOption, Name ?? string.Empty, UrlOfPage ?? string.Empty, context);
        var success = await organisationClient.FeedbackAndContact(feedback);

        return RedirectToPage("YourDetails", new { RedirectUri = Helper.ValidRelativeUri(redirectUri) ? redirectUri : default });
    }
}