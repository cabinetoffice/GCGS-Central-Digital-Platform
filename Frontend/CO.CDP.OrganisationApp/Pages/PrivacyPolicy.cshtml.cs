using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages;

public class PrivacyPolicyModel(ISession session) : LoggedInUserAwareModel(session)
{
    [BindProperty]
    [DisplayName(nameof(StaticTextResource.PrivacyPolicy_YesIHaveReadAndAgreePrivacyPolicy))]
    [Required(ErrorMessageResourceName=nameof(StaticTextResource.PrivacyPolicy_SelectReadAndAgreeToPrivacyPolicy), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? AgreeToPrivacy { get; set; }

    public IActionResult OnGet()
    {
        if (UserDetails.PersonId.HasValue)
        {
            return RedirectToPage("Organisation/OrganisationSelection");
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