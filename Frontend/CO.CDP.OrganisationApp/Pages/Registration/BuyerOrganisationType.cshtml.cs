using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
[ValidateRegistrationStep]
public class BuyerOrganisationTypeModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => BuyerOrganisationTypePage;

    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Select the organisation type")]
    public string? BuyerOrganisationType { get; set; }

    //[BindProperty]
    //[DisplayName("Enter type")]
    //[RequiredIf("BuyerOrganisationType", "Other")]
    //public string? OtherValue { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        BuyerOrganisationType = RegistrationDetails.BuyerOrganisationType;
        //OtherValue = RegistrationDetails.BuyerOrganisationOtherValue;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.BuyerOrganisationType = BuyerOrganisationType;

        //RegistrationDetails.BuyerOrganisationType = (BuyerOrganisationType == "Other" ? OtherValue : BuyerOrganisationType);
        //RegistrationDetails.BuyerOrganisationOtherValue = (BuyerOrganisationType == "Other" ? OtherValue : "");
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

    public static string BuyerTypeDescription(string buyerType)
    {
        return buyerType switch
        {
            "CentralGovernment" => "Central government, public authority: UK, Scottish, Welsh and Northern Irish Executive",
            "RegionalAndLocalGovernment" => "Regional and local government, public authority: UK, Scottish, Welsh and Northern Irish",
            "PublicUndertaking" => "Public undertaking",
            "PrivateUtility" => "Private utility",
            "Other" => "Other",
            _ => throw new NotImplementedException(),
        };
    }
}