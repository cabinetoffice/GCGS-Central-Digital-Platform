using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class BuyerDevolvedRegulationModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => BuyerDevolvedRegulationPage;

    [BindProperty]
    [Required(ErrorMessage = "Select 'yes' or 'no'")]
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
            SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            return RedirectToPage("BuyerSelectDevolvedRegulation");
        }
        else
        {
            RegistrationDetails.Regulations = [];
            SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            return RedirectToPage("OrganisationDetailsSummary");
        }
    }
}
