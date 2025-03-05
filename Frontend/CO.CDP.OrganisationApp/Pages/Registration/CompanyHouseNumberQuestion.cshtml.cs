using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class CompanyHouseNumberQuestionModel(ISession session,
    ICompaniesHouseApi companiesHouseApi,
    IOrganisationClient organisationClient,
    IFlashMessageService flashMessageService) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationHasCompanyHouseNumberPage;

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_ValidationErrorMessage))]
    public bool? HasCompaniesHouseNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasCompaniesHouseNumber), true, ErrorMessage = nameof(StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_ErrorMessage))]
    public string? CompaniesHouseNumber { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public string? Error { get; set; }

    [BindProperty]
    public string? FailedCompaniesHouseNumber { get; set; }

    public string? OrganisationIdentifier;

    public string? OrganisationName;

    public void OnGet()
    {
        HasCompaniesHouseNumber = RegistrationDetails.OrganisationHasCompaniesHouseNumber;
        CompaniesHouseNumber = RegistrationDetails.OrganisationIdentificationNumber;
    }

    public async Task<IActionResult> OnPost()
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
            OrganisationIdentifier = $"GB-COH:{RegistrationDetails.OrganisationIdentificationNumber}";

            try
            {
                var organisation = await organisationClient.LookupOrganisationAsync(string.Empty, OrganisationIdentifier);

                OrganisationName = organisation?.Name;
                if (organisation != null)
                {                    
                    SessionContext.Set(Session.JoinOrganisationRequest,
                        new JoinOrganisationRequestState { OrganisationId = organisation.Id, OrganisationName = organisation.Name }
                        );

                    flashMessageService.SetFlashMessage(
                        FlashMessageType.Important,
                        heading: StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyAlreadyRegistered_NotificationBanner,
                        urlParameters: new() { ["organisationIdentifier"] = organisation.Id.ToString() },
                        htmlParameters: new() { ["organisationName"] = organisation.Name }
                    );
                }

                return Page();
            }
            catch (Exception orgApiException) when (orgApiException is ApiException && ((ApiException)orgApiException).StatusCode == 404)
            {
                if (FailedCompaniesHouseNumber != CompaniesHouseNumber)
                {
                    var chProfile = await companiesHouseApi.GetProfile(RegistrationDetails.OrganisationIdentificationNumber!);

                    if (chProfile == null)
                    {
                        FailedCompaniesHouseNumber = CompaniesHouseNumber;

                        flashMessageService.SetFlashMessage(FlashMessageType.Important, StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyNotFound_NotificationBanner);
                        return Page();
                    }
                }
            }
        }

        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

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
