using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class CompanyHouseNumberQuestionModel(ISession session,
    ICompaniesHouseApi companiesHouseApi,
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationHasCompanyHouseNumberPage;

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasCompaniesHouseNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasCompaniesHouseNumber), true, ErrorMessage = "Please enter the Companies House number.")]
    public string? CompaniesHouseNumber { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public string? Error { get; set; }

    [BindProperty]
    public string? FailedCompaniesHouseNumber { get; set; }

    public Guid? OrganisationId;

    public string? OrganisationName;

    public FlashMessage NotificationBannerCompanyNotFound { get { return new FlashMessage("We cannot find your company number on Companies House. If it’s correct continue and enter your details manually."); } }
    public FlashMessage NotificationBannerCompanyAlreadyRegistered { get { return new FlashMessage("An organisation with this company number already exists. Change the company number or <a class='govuk-notification-banner__link' href='/registration/" + OrganisationId + "/join-organisation'>request to join " + OrganisationName + ".</a>"); } }

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

            try
            {
                var organisation = await organisationClient.LookupOrganisationAsync(string.Empty, $"GB-COH:{RegistrationDetails.OrganisationIdentificationNumber}");
                OrganisationId = organisation?.Id;
                OrganisationName = organisation?.Name;
                tempDataService.Put(FlashMessageTypes.Important, NotificationBannerCompanyAlreadyRegistered);
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
                        tempDataService.Put(FlashMessageTypes.Important, NotificationBannerCompanyNotFound);
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