using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class BuyerOrganisationTypeModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => BuyerOrganisationTypePage;

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.OrganisationRegistration_BuyerOrganisationType_ValidationErrorMessage))]
    public string? BuyerOrganisationType { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_BuyerOrganisationType_OtherEnterType_Label))]
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
        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

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
        { "CentralGovernment", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_CentralGovernment_Label},
        { "RegionalAndLocalGovernment", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_RegionalAndLocalGovernment_Label},
        { "PublicUndertaking", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_PublicUndertaking_Label},
        { "PrivateUtility", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_PrivateUtility_Label}
    };
}