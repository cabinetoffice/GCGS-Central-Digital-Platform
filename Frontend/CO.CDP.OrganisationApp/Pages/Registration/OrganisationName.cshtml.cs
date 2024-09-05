using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationNameModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationNamePage;

    [BindProperty]
    [DisplayName("Enter the organisation's name")]
    [Required(ErrorMessage = "Enter the organisation's name")]
    public string? OrganisationName { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public bool HasCompaniesHouseNumber { get; set; }

    public void OnGet()
    {
        OrganisationName = RegistrationDetails.OrganisationName;
        HasCompaniesHouseNumber = RegistrationDetails.OrganisationHasCompaniesHouseNumber ?? false;
    }

    public IActionResult OnPost()
    {
        HasCompaniesHouseNumber = RegistrationDetails.OrganisationHasCompaniesHouseNumber ?? false;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationName = OrganisationName;
        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationEmail");
        }
    }
}