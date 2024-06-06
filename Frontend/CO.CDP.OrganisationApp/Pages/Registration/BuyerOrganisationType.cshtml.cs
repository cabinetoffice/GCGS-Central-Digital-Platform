using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
[ValidateRegistrationStep]
public class BuyerOrganisationTypeModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => BuyerOrganisationTypePage;

    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Select the organisation type")]
    public string? BuyerOrganisationType { get; set; }

    [BindProperty]
    [DisplayName("Enter type")]
    [RequiredIf("BuyerOrganisationType", "type5")]
    public string? OtherValue { get; set; }

    public IActionResult OnGet()
    {
        BuyerOrganisationType = RegistrationDetails.BuyerOrganisationType;
        OtherValue = RegistrationDetails.BuyerOrganisationOtherValue;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.BuyerOrganisationType = BuyerOrganisationType;
        RegistrationDetails.BuyerOrganisationOtherValue = (BuyerOrganisationType == "type5" ? OtherValue : "");
        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        return RedirectToPage("BuyerDevolvedRegulation");

    }
}