using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Mvc.Validation;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class BuyerOrganisationTypeModel(
    ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the organisation type")]
    public string? BuyerOrganisationType { get; set; }

    [BindProperty]
    [DisplayName("Enter type")]
    [RequiredIf("BuyerOrganisationType", "type5")]
    public string? OtherValue { get; set; }

    public IActionResult OnGet()
    {
        var registrationDetails = VerifySession();

        BuyerOrganisationType = registrationDetails.BuyerOrganisationType;
        OtherValue = registrationDetails.BuyerOrganisationOtherValue;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();
        registrationDetails.BuyerOrganisationType = BuyerOrganisationType;
        registrationDetails.BuyerOrganisationOtherValue = (BuyerOrganisationType == "type5" ? OtherValue : "");
        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("BuyerDevolvedRegulation");

    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}