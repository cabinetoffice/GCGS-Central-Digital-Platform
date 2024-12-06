using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
public class ConsortiumEmailModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Consortium_ConsortiumEmail_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Consortium_ConsortiumEmail_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EmailAddress { get; set; }

    public string? ConsortiumName { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();

        if (!valid)
        {
            return RedirectToPage("ConsortiumStart");
        }

        InitModel(state, true);

        return Page();
    }
    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage("ConsortiumStart");
        }

        InitModel(state);

        if (!ModelState.IsValid) return Page();

        var cd = session.Get<ConsortiumState>(Session.ConsortiumKey) ?? new ConsortiumState();

        cd.ConstortiumEmail = EmailAddress;

        session.Set(Session.ConsortiumKey, cd);

        //TODO: Need to call API to save into DB

        return RedirectToPage("ConsortiumOverview");
    }

    private void InitModel(ConsortiumState state, bool reset = false)
    {
        ConsortiumName = state.ConstortiumName;

        if (reset)
        {
            EmailAddress = state.ConstortiumEmail;
        }        
    }

    private (bool valid, ConsortiumState state) ValidatePage()
    {
        var cd = session.Get<ConsortiumState>(Session.ConsortiumKey);

        if (cd == null)
        {
            return (false, new());
        }

        return (true, cd);
    }
}
