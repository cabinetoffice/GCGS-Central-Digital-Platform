using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationNameModel(ISession session, ICharityCommissionApi charityCommissionApi, ICompaniesHouseApi companiesHouseApi) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationNamePage;

    [BindProperty(SupportsGet = true)]
    public bool InternationalIdentifier { get; set; } = false;

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading), ErrorMessageResourceType = typeof(StaticTextResource))]
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

        if ((RegistrationDetails.OrganisationScheme == OrganisationSchemeType.CharityCommissionEnglandWales) && (string.IsNullOrEmpty(OrganisationName)))
        {
            if (RegistrationDetails.OrganisationIdentificationNumber != null)
            {
                var details = await charityCommissionApi.GetCharityDetails(RegistrationDetails.OrganisationIdentificationNumber);

                if (details != null)
                {
                    OrganisationName = details.Name;
                }
            }
        }
    }

    public IActionResult OnPost()
    {
        HasCompaniesHouseNumber = RegistrationDetails.OrganisationHasCompaniesHouseNumber ?? false;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationName = OrganisationName?.Trim();
        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationNameSearch");
        }
    }
}