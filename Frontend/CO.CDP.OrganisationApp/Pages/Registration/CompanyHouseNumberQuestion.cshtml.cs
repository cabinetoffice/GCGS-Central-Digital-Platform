using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class CompanyHouseNumberQuestionModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasCompaniesHouseNumber { get; set; }

    [BindProperty]
    public string? CompaniesHouseNumber { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();
        HasCompaniesHouseNumber = registrationDetails.OrganisationHasCompaniesHouseNumber;
        CompaniesHouseNumber = registrationDetails.OrganisationIdentificationNumber;
    }

    public IActionResult OnPost()
    {
        if (HasCompaniesHouseNumber == true && string.IsNullOrWhiteSpace(CompaniesHouseNumber))
        {
            ModelState.AddModelError(nameof(CompaniesHouseNumber), "Please enter the Companies House number.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.OrganisationHasCompaniesHouseNumber = HasCompaniesHouseNumber;
        registrationDetails.OrganisationIdentificationNumber = CompaniesHouseNumber;

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else if (HasCompaniesHouseNumber == false)
        {
            return RedirectToPage("OrganisationIdentification");
        }
        else
        {
            return RedirectToPage("OrganisationName");
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
