using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace CO.CDP.OrganisationApp.Pages;

using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;


[AuthenticatedSessionNotRequired]
public class ProvideFeedbackAndContact(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [DisplayName("What's it to do with")]
    [Required(ErrorMessage = "Choose a option")]
    public string? FeedbackOrContactOption { get; set; }

    [BindProperty]
    [DisplayName("Specific Page")]
    [RequiredIf("FeedbackOrContactOption", "SpecificPage", ErrorMessage = "URL of specific page is required")]
    public string? UrlOfPage { get; set; }

    [BindProperty]
    [DisplayName("Enter details")]
    [Required(ErrorMessage = "The message field cannot be empty")]
    public string? Details { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public string? Email { get; set; }

    public required string Context { get; set; }

    public IActionResult OnGet()
    {
        SetContext();
        return Page();
    }

    private void SetContext()
    {
        Context = Request.Query["context"].ToString().ToUpper();
        if (ValidateContext() == false) throw new Exception("Unknown context");
    }

    private bool ValidateContext()
    {
        if (string.IsNullOrEmpty(Context) || (Context != "FEEDBACK" && Context != "CONTACT"))
            return false;

        return true;
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

        SetContext();

        if (ValidateContext() == false) throw new Exception("Unknown context");

        var feedback = new CO.CDP.Organisation.WebApiClient.ProvideFeedbackAndContact(Email ?? string.Empty, Details, FeedbackOrContactOption, Name ?? string.Empty, UrlOfPage ?? string.Empty, Context);
        var success = await organisationClient.FeedbackAndContact(feedback);

        return RedirectToPage("confirmationmessage");
    }
}