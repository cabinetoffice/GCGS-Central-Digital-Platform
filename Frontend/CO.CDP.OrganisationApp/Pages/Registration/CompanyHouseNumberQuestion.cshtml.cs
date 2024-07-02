using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
[ValidateRegistrationStep]
public class CompanyHouseNumberQuestionModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationHasCompanyHouseNumberPage;
    public override ISession SessionContext => session;

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
        HasCompaniesHouseNumber = RegistrationDetails.OrganisationHasCompaniesHouseNumber;
        CompaniesHouseNumber = RegistrationDetails.OrganisationIdentificationNumber;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationHasCompaniesHouseNumber = HasCompaniesHouseNumber;
        if (HasCompaniesHouseNumber ?? false)
        {
            RegistrationDetails.OrganisationIdentificationNumber = CompaniesHouseNumber;
            RegistrationDetails.OrganisationScheme = "GB-COH";
        }

        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

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
}
