using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
public class ConsortiumNameModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Consortium_ConsortiumName_EnterNameError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? ConsortiumName { get; set; }

    public IActionResult OnGet()
    {
        var cd = session.Get<ConsortiumState>(Session.ConsortiumKey) ?? new ConsortiumState();

        session.Set(Session.ConsortiumKey, cd);

        ConsortiumName = cd.ConstortiumName;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var cd = session.Get<ConsortiumState>(Session.ConsortiumKey) ?? new ConsortiumState();

        cd.ConstortiumName = ConsortiumName;

        session.Set(Session.ConsortiumKey, cd);

        return RedirectToPage("ConsortiumAddress", new { UkOrNonUk = "uk" });
    }
}
