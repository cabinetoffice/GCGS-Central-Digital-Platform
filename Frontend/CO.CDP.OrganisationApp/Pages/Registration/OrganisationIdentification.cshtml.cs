using CO.CDP.EntityVerificationClient;
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
    IPponClient pponClient) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationIdentifierPage;

    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required(ErrorMessage = "Select an option")]
    public string? OrganisationScheme { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for England and Wales")]
    public string? CharityCommissionEnglandWales { get; set; }

    [BindProperty]
    [DisplayName("Charity number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-CHC", ErrorMessage = "Enter the number")]
    public string? CharityCommissionEnglandWalesNumber { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator")]
    public string? ScottishCharityRegulator { get; set; }

    [BindProperty]
    [DisplayName("Charity number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-SC", ErrorMessage = "Enter the number")]
    public string? ScottishCharityRegulatorNumber { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northern Ireland")]
    public string? CharityCommissionNorthernIreland { get; set; }

    [BindProperty]
    [DisplayName("Charity number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-NIC", ErrorMessage = "Enter the number")]
    public string? CharityCommissionNorthernIrelandNumber { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register")]
    public string? MutualsPublicRegister { get; set; }

    [BindProperty]
    [DisplayName("Registration number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-MPR", ErrorMessage = "Enter the number")]
    public string? MutualsPublicRegisterNumber { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry")]
    public string? GuernseyRegistry { get; set; }

    [BindProperty]
    [DisplayName("Entity number")]
    [RequiredIf(nameof(OrganisationScheme), "GG-RCE", ErrorMessage = "Enter the number")]
    public string? GuernseyRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry")]
    public string? JerseyFinancialServicesCommissionRegistry { get; set; }

    [BindProperty]
    [DisplayName("Registration number")]
    [RequiredIf(nameof(OrganisationScheme), "JE-FSC", ErrorMessage = "Enter the number")]
    public string? JerseyFinancialServicesCommissionRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry")]
    public string? IsleofManCompaniesRegistry { get; set; }

    [BindProperty]
    [DisplayName("Company number")]
    [RequiredIf(nameof(OrganisationScheme), "IM-CR", ErrorMessage = "Enter the number")]
    public string? IsleofManCompaniesRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("NHS Organisation Data Service (ODS)")]
    public string? NationalHealthServiceOrganisationsRegistry { get; set; }

    [BindProperty]
    [DisplayName("ODS code")]
    [RequiredIf(nameof(OrganisationScheme), "GB-NHS", ErrorMessage = "Enter the code")]
    public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Providers")]
    public string? UKLearningProviderReference { get; set; }

    [BindProperty]
    [DisplayName("UK Provider Reference Number (UKPRN)")]
    [RequiredIf(nameof(OrganisationScheme), "GB-UKPRN", ErrorMessage = "Enter the number")]
    public string? UKLearningProviderReferenceNumber { get; set; }

    [BindProperty]
    [DisplayName("None apply")]
    public string? Other { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

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
        try
        {
            SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            await LookupOrganisationAsync();
        }
        catch (Exception orgApiException) when (orgApiException is CO.CDP.Organisation.WebApiClient.ApiException && ((CO.CDP.Organisation.WebApiClient.ApiException)orgApiException).StatusCode == 404)
        {
            try
            {
                await LookupEntityVerificationAsync();
            }
            catch (Exception evApiException) when (evApiException is EntityVerificationClient.ApiException eve && eve.StatusCode == 404)
            {
                if (RedirectToSummary == true)
                {
                    return RedirectToPage("OrganisationDetailsSummary");
                }
                else
                {
                    return RedirectToPage("OrganisationName");
                }
            }
            catch
            {
                return RedirectToPage("OrganisationRegistrationUnavailable");
            }
        }

        return RedirectToPage("OrganisationAlreadyRegistered");
    }

    private async Task<CO.CDP.Organisation.WebApiClient.Organisation> LookupOrganisationAsync()
    {
        return await organisationClient.LookupOrganisationAsync(string.Empty,
                    $"{OrganisationScheme}:{RegistrationDetails.OrganisationIdentificationNumber}");
    }

    private async Task<ICollection<EntityVerificationClient.Identifier>> LookupEntityVerificationAsync()
    {
        return await pponClient.GetIdentifiersAsync($"{OrganisationScheme}:{RegistrationDetails.OrganisationIdentificationNumber}");
    }
}
