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

    public IActionResult OnGet()
    {   
        ConsortiumName = ConsortiumDetails.ConstortiumName;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        ConsortiumDetails.ConstortiumName = ConsortiumName;

        SessionContext.Set(Session.ConsortiumKey, ConsortiumDetails);

        return RedirectToPage("ConsortiumAddress", new { UkOrNonUk = "uk" });
    }
}
