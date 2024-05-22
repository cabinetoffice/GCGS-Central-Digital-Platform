using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationNameModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Enter the organisation's name")]
    [Required(ErrorMessage = "Enter the organisation's name")]
    public string? OrganisationName { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public bool HasCompaniesHouseNumber { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();

        OrganisationName = registrationDetails.OrganisationName;
        HasCompaniesHouseNumber = registrationDetails.OrganisationHasCompaniesHouseNumber ?? false;
    }

    public IActionResult OnPost()
    {
        var registrationDetails = VerifySession();
        HasCompaniesHouseNumber = registrationDetails.OrganisationHasCompaniesHouseNumber ?? false;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        registrationDetails.OrganisationName = OrganisationName;
        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationEmail");
        }
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);

        if (registrationDetails == null)
        {
            //show error page (Once we finalise)
            throw new Exception("Shoudn't be here");
        }

        return registrationDetails;
    }
}