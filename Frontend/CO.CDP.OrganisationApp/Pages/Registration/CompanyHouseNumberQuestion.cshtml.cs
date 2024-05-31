using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Mvc.Validation;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class CompanyHouseNumberQuestionModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasCompaniesHouseNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasCompaniesHouseNumber), true, ErrorMessage = "Please enter the Companies House number.")]
    public string? CompaniesHouseNumber { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public string? Error { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();
        HasCompaniesHouseNumber = registrationDetails.OrganisationHasCompaniesHouseNumber;
        CompaniesHouseNumber = registrationDetails.OrganisationIdentificationNumber;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        registrationDetails.OrganisationHasCompaniesHouseNumber = HasCompaniesHouseNumber;
        if (HasCompaniesHouseNumber ?? false)
        {
            registrationDetails.OrganisationIdentificationNumber = CompaniesHouseNumber;
            registrationDetails.OrganisationScheme = "CHN";
        }

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        if (HasCompaniesHouseNumber == false)
        {
            var routeValuesDictionary = new RouteValueDictionary();
            if (RedirectToSummary ?? false)
            {
                routeValuesDictionary.Add("frm-summary", "true");
            }

            return RedirectToPage($"OrganisationIdentification", routeValuesDictionary);
        }
        else if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationName");
        }
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}
