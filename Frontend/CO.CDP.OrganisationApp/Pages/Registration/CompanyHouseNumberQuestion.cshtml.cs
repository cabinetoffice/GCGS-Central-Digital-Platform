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
    [RegularExpression(@"^(?:[A-Z]{2}\d{6}|\d{8})$", ErrorMessage = "CRN is made up of up to eight digits or two alphabetical characters followed by 6 digits.")]
    [RequiredIf(nameof(HasCompaniesHouseNumber), true, ErrorMessage = "Please enter the Companies House number.")]
    public string? CompaniesHouseNumber { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public string? Error { get; set; }

    [BindProperty]
    public string? FailedCompaniesHouseNumber { get; set; }

    public static string NotificationBannerCompanyNotFound { get { return "We cannot find your company number on Companies House. If it’s correct you may continue and enter your details manually."; } }
    public static string NotificationBannerCompanyHouseApiError { get { return "We are unable to verify the Companies House number at present. You may continue or try adding your organisation again later."; } }
    public static string NotificationBannerCompanyAlreadyRegistered { get { return "An organisation with this company number already exists. Change the company number or <a class='govuk-notification-banner__link' href='#'>request to join the organisation.</a>"; } }

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
                await organisationClient.LookupOrganisationAsync(string.Empty, $"GB-COH:{RegistrationDetails.OrganisationIdentificationNumber}");

                tempDataService.Put(FlashMessageTypes.Important, NotificationBannerCompanyAlreadyRegistered);
                return Page();
            }
            catch (Exception orgApiException) when (orgApiException is ApiException && ((ApiException)orgApiException).StatusCode == 404)
            {
                if (FailedCompaniesHouseNumber != CompaniesHouseNumber)
                {
                    var (chProfile, httpStatus) = await companiesHouseApi.GetProfile(RegistrationDetails.OrganisationIdentificationNumber!);
                    
                    if (httpStatus == System.Net.HttpStatusCode.NotFound)
                    {
                        FailedCompaniesHouseNumber = CompaniesHouseNumber;
                        tempDataService.Put(FlashMessageTypes.Important, NotificationBannerCompanyNotFound);
                        return Page();
                    }

                    if ((httpStatus == System.Net.HttpStatusCode.ServiceUnavailable) || (httpStatus == System.Net.HttpStatusCode.InternalServerError))
                    {
                        FailedCompaniesHouseNumber = CompaniesHouseNumber;
                        tempDataService.Put(FlashMessageTypes.Important, NotificationBannerCompanyHouseApiError);
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