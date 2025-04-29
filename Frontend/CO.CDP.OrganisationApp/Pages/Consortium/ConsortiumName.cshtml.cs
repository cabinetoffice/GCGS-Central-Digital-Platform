using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
[ValidateConsortiumStep]
public class ConsortiumNameModel(ISession session) : ConsortiumStepModel(session)
{
    public override string CurrentPage => ConsortiumNamePage;

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Consortium_ConsortiumName_EnterNameError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? ConsortiumName { get; set; }

    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectToCheckYourAnswer { get; set; }
    public IActionResult OnGet()
    {   
        ConsortiumName = ConsortiumDetails.ConsortiumName;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        ConsortiumDetails.ConsortiumName = ConsortiumName;

        SessionContext.Set(Session.ConsortiumKey, ConsortiumDetails);

        if (RedirectToCheckYourAnswer == true)
        {
            return RedirectToPage("ConsortiumCheckAnswer");
        }
        else
        {
            return RedirectToPage("ConsortiumAddress", new { UkOrNonUk = "uk" });
        }
    }
}