using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class BuyerSelectDevolvedRegulationModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => BuyerSelectDevolvedRegulationPage;

    [BindProperty]
    [NotEmpty(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_BuyerSelectDevolvedRegulation_ValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
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
        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        return RedirectToPage("OrganisationDetailsSummary");
    }
}
