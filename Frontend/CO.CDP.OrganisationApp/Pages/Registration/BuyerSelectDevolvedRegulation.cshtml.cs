using CO.CDP.Mvc.Validation;
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
    public required List<string> Regulations { get; set; } = [];

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

        return RedirectToPage("/OrganisationSelection");
    }
}