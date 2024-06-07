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
    [RequiredIf("BuyerOrganisationType", "Other")]
    public string? OtherValue { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        BuyerOrganisationType = RegistrationDetails.BuyerOrganisationType;
        if (!string.IsNullOrEmpty(BuyerOrganisationType) && !BuyerTypes.Keys.Contains(BuyerOrganisationType))
        {
            OtherValue = BuyerOrganisationType;
            BuyerOrganisationType = "Other";
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.BuyerOrganisationType = (BuyerOrganisationType == "Other" ? OtherValue : BuyerOrganisationType);
        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("BuyerDevolvedRegulation");
        }

    }

    public static Dictionary<string, string> BuyerTypes => new()
    {
        { "CentralGovernment", "Central government, public authority: UK, Scottish, Welsh and Northern Irish Executive"},
        { "RegionalAndLocalGovernment", "Regional and local government, public authority: UK, Scottish, Welsh and Northern Irish"},
        { "PublicUndertaking", "Public undertaking"},
        { "PrivateUtility", "Private utility"}
    };
}