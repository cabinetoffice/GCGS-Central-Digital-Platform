using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationNameModel(ISession session, ICompaniesHouseApi companiesHouseApi) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationNamePage;

    [BindProperty]
    [DisplayName(nameof(ValidationMessagesResource.OrganisationRegistration_EnterOrganisationName))]
    [Required(ErrorMessage = nameof(ValidationMessagesResource.OrganisationRegistration_EnterOrganisationName))]
    public string? OrganisationName { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public bool HasCompaniesHouseNumber { get; set; }

    public async Task OnGet()
    {
        OrganisationName = RegistrationDetails.OrganisationName;
        HasCompaniesHouseNumber = RegistrationDetails.OrganisationHasCompaniesHouseNumber ?? false;

        if (HasCompaniesHouseNumber && string.IsNullOrEmpty(OrganisationName))
        {
            var profile = await companiesHouseApi.GetProfile(RegistrationDetails.OrganisationIdentificationNumber!);

            OrganisationName = profile != null ? profile.CompanyName ?? string.Empty : string.Empty;
        }
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