using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
[ValidateRegistrationStep]
public class BuyerDevolvedRegulationModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => BuyerDevolvedRegulationPage;

    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Select the do devolved regulations apply to your organisation?")]
    public bool? Devolved { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        Devolved = RegistrationDetails.Devolved;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.Devolved = Devolved;

        if (Devolved == true)
        {
            session.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            return RedirectToPage("BuyerSelectDevolvedRegulation");
        }
        else
        {
            RegistrationDetails.Regulations = [];
            session.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            return RedirectToPage("OrganisationDetailsSummary");
        }
    }
}