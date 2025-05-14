using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
[ValidateConsortiumStep]
public class ConsortiumEmailModel(
    ISession session) : ConsortiumStepModel(session)
{
    public override string CurrentPage => ConsortiumEmailPage;

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Consortium_ConsortiumEmail_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Consortium_ConsortiumEmail_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ModelBinder<SanitisedStringModelBinder>]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EmailAddress { get; set; }

    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectToCheckYourAnswer { get; set; }
    public string? ConsortiumName => ConsortiumDetails.ConsortiumName;

    public IActionResult OnGet()
    {
        EmailAddress = ConsortiumDetails.ConsortiumEmail;

        return Page();
    }
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        ConsortiumDetails.ConsortiumEmail = EmailAddress;

        SessionContext.Set(Session.ConsortiumKey, ConsortiumDetails);

        return RedirectToPage("ConsortiumCheckAnswer");
    }
}