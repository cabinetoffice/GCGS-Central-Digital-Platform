using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages;


[AuthenticatedSessionNotRequired]
public class ProvideFeedbackAndContact(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [DisplayName(nameof(StaticTextResource.ProvideFeedbackAndContact_WhatsItToDoWith))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.ProvideFeedbackAndContact_ChooseAnOption), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? FeedbackOrContactOption { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.ProvideFeedbackAndContact_SpecificPage))]
    [RequiredIf("FeedbackOrContactOption", "SpecificPage", ErrorMessageResourceName = nameof(StaticTextResource.ProvideFeedbackAndContact_UrlOfPageRequired), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? UrlOfPage { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.ProvideFeedbackAndContact_EnterDetails))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.ProvideFeedbackAndContact_MessageFieldCannotBeEmpty), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Details { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty(SupportsGet = true)]
    public required ContactFormTypes Context { get; set; }

    public IActionResult OnGet()
    {
        ValidateContext();
        return Page();
    }

    private void ValidateContext()
    {
        if(Context == ContactFormTypes.None)
        {
            throw new Exception("Unknown context");
        }
    }

    public async Task<IActionResult> OnPost(string? redirectUri = null)
    {
        if ((!string.IsNullOrEmpty(ModelState[nameof(Name)]!.RawValue as string)) &&
            (ModelState[nameof(Email)]!.RawValue as string == string.Empty))
        {
            ModelState[nameof(Email)]!.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid;
            ModelState[nameof(Email)]!.Errors.Add(StaticTextResource.ProvideFeedbackAndContact_EnterEmailAddress);
            return Page();
        }

        if ((!string.IsNullOrEmpty(ModelState[nameof(Email)]!.RawValue as string)) &&
            (ModelState[nameof(Name)]!.RawValue as string == string.Empty))
        {
            ModelState[nameof(Name)]!.ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid;
            ModelState[nameof(Name)]!.Errors.Add(StaticTextResource.ProvideFeedbackAndContact_EnterAName);
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ValidateContext();

        var feedback = new CO.CDP.Organisation.WebApiClient.ProvideFeedbackAndContact(Email ?? string.Empty, Details, FeedbackOrContactOption, Name ?? string.Empty, UrlOfPage ?? string.Empty, Context.ToString().ToUpper());
        var success = await organisationClient.FeedbackAndContact(feedback);

        return RedirectToPage("confirmationmessage");
    }
}

public enum ContactFormTypes
{
    None,
    Support,
    Feedback
}