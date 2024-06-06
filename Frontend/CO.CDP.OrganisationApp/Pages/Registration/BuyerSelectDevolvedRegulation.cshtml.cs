using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
[ValidateRegistrationStep]
public class BuyerSelectDevolvedRegulationModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => BuyerSelectDevolvedRegulationPage;

    public override ISession SessionContext => session;

    [BindProperty]
    [NotEmpty(ErrorMessage = "Select the do devolved regulations apply to your organisation?")]
    public required List<DevolvedRegulation> Regulations { get; set; } = [];

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        Regulations = RegistrationDetails.Regulations;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.Regulations = Regulations;
        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        return RedirectToPage("OrganisationDetailsSummary");
    }
}