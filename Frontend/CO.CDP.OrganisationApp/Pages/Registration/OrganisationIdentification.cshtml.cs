using CO.CDP.EntityVerificationClient;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationIdentificationModel(ISession session,
    IOrganisationClient organisationClient,
    IPponClient pponClient,
    IFlashMessageService flashMessageService) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationIdentifierPage;

    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required(ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_ErrorMessage))]
    public string? OrganisationScheme { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_CHC_Label))]
    public string? CharityCommissionEnglandWales { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Charity_Number_Label))]
    [RequiredIf(nameof(OrganisationScheme), "GB-CHC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? CharityCommissionEnglandWalesNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_SC_Label))]
    public string? ScottishCharityRegulator { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Charity_Number_Label))]
    [RequiredIf(nameof(OrganisationScheme), "GB-SC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? ScottishCharityRegulatorNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NIC_Label))]
    public string? CharityCommissionNorthernIreland { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Charity_Number_Label))]
    [RequiredIf(nameof(OrganisationScheme), "GB-NIC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? CharityCommissionNorthernIrelandNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_MPR_Label))]
    public string? MutualsPublicRegister { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Registration_Number_Label))]
    [RequiredIf(nameof(OrganisationScheme), "GB-MPR", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? MutualsPublicRegisterNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GG_RCE_Label))]
    public string? GuernseyRegistry { get; set; }

    [BindProperty]
    [DisplayName("Entity number")]
    [RequiredIf(nameof(OrganisationScheme), "GG-RCE", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? GuernseyRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_JE_FSC_Label))]
    public string? JerseyFinancialServicesCommissionRegistry { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Registration_Number_Label))]
    [RequiredIf(nameof(OrganisationScheme), "JE-FSC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? JerseyFinancialServicesCommissionRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_IM_CR_Label))]
    public string? IsleofManCompaniesRegistry { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Company_Number_Label))]
    [RequiredIf(nameof(OrganisationScheme), "IM-CR", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? IsleofManCompaniesRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NHS_Label))]
    public string? NationalHealthServiceOrganisationsRegistry { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_ODS_Code_Label))]
    [RequiredIf(nameof(OrganisationScheme), "GB-NHS", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_ODS_Code_ErrorMessage))]
    public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_UKPRN_Label))]
    public string? UKLearningProviderReference { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_UKPRN_Number_Label))]
    [RequiredIf(nameof(OrganisationScheme), "GB-UKPRN", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_Number_ErrorMessage))]
    public string? UKLearningProviderReferenceNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Other_Label))]
    public string? Other { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public string? Identifier { get; set; }

    public string? OrganisationName;

    public Guid? OrganisationId { get; set; }

    public void OnGet()
    {
        OrganisationScheme = RegistrationDetails.OrganisationScheme;

        switch (OrganisationScheme)
        {
            case "GB-CHC":
                CharityCommissionEnglandWalesNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "GB-SC":
                ScottishCharityRegulatorNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "GB-NIC":
                CharityCommissionNorthernIrelandNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "GB-MPR":
                MutualsPublicRegisterNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "GG-RCE":
                GuernseyRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;

            case "JE-FSC":
                JerseyFinancialServicesCommissionRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "IM-CR":
                IsleofManCompaniesRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "GB-NHS":
                NationalHealthServiceOrganisationsRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "GB-UKPRN":
                UKLearningProviderReferenceNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            default:
                break;
        }

    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationScheme = OrganisationScheme;

        RegistrationDetails.OrganisationIdentificationNumber = OrganisationScheme switch
        {
            "GB-CHC" => CharityCommissionEnglandWalesNumber,
            "GB-SC" => ScottishCharityRegulatorNumber,
            "GB-NIC" => CharityCommissionNorthernIrelandNumber,
            "GB-MPR" => MutualsPublicRegisterNumber,
            "GG-RCE" => GuernseyRegistryNumber,
            "JE-FSC" => JerseyFinancialServicesCommissionRegistryNumber,
            "IM-CR" => IsleofManCompaniesRegistryNumber,
            "GB-NHS" => NationalHealthServiceOrganisationsRegistryNumber,
            "GB-UKPRN" => UKLearningProviderReferenceNumber,
            "Other" => null,
            _ => null,
        };

        Identifier = $"{OrganisationScheme}:{RegistrationDetails.OrganisationIdentificationNumber}";

        if (RegistrationDetails.OrganisationScheme == "Other")
        {
            SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            return RedirectToPage(RedirectToSummary == true ? "OrganisationDetailsSummary" : "OrganisationName");
        }

        try
        {
            SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            var organisation = await LookupOrganisationAsync();
            OrganisationName = organisation?.Name;
            OrganisationId = organisation?.Id;
        }
        catch (Exception orgApiException) when (orgApiException is CO.CDP.Organisation.WebApiClient.ApiException && ((CO.CDP.Organisation.WebApiClient.ApiException)orgApiException).StatusCode == 404)
        {
            try
            {
                await LookupEntityVerificationAsync();
            }
            catch (Exception evApiException) when (evApiException is EntityVerificationClient.ApiException eve && eve.StatusCode == 404)
            {
                return RedirectToPage(RedirectToSummary == true ? "OrganisationDetailsSummary" : "OrganisationName");
            }
            catch
            {
                return RedirectToPage("OrganisationRegistrationUnavailable");
            }
        }

        if (OrganisationName != null && OrganisationId != null)
        {
            SessionContext.Set(Session.JoinOrganisationRequest,
                    new JoinOrganisationRequestState { OrganisationId = OrganisationId, OrganisationName = OrganisationName }
                    );

            flashMessageService.SetFlashMessage(
                FlashMessageType.Important,
                heading: "An organisation with this registration number already exists. Change the registration number or <a class='govuk-notification-banner__link' href='/registration/{identifier}/join-organisation'>request to join {organisationName}.</a>",
                urlParameters: new() { ["identifier"] = OrganisationId.Value.ToString() },
                htmlParameters: new() { ["organisationName"] = OrganisationName }
            );
        }

        return Page();

    }

    private async Task<CO.CDP.Organisation.WebApiClient.Organisation> LookupOrganisationAsync()
    {
        return await organisationClient.LookupOrganisationAsync(string.Empty, Identifier);
    }

    private async Task<ICollection<EntityVerificationClient.Identifier>> LookupEntityVerificationAsync()
    {
        var result = await pponClient.GetIdentifiersAsync(Identifier);

        OrganisationId = result.First().OrganisationId;

        return result;
    }
}
