using CO.CDP.Common.Enums;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationTypeModel(
    ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the organisation type")]
    public OrganisationType? OrganisationType { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        var registrationDetails = VerifySession();

        OrganisationType = registrationDetails.OrganisationType;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();
        registrationDetails.OrganisationType = OrganisationType ?? registrationDetails.OrganisationType;
        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("CompanyHouseNumberQuestion");
        }
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}