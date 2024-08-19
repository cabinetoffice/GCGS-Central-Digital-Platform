using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
[ValidateRegistrationStep]
public class OrganisationIdentificationModel(ISession session,
    IOrganisationClient organisationClient) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationIdentifierPage;
    public override ISession SessionContext => session;

    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required(ErrorMessage = "Please select your organisation type")]
    public string? OrganisationScheme { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for England & Wales")]
    public string? CharityCommissionEnglandWales { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for England & Wales Number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-CHC", ErrorMessage = "Please enter the Charity Commission for England & Wales number.")]
    public string? CharityCommissionEnglandWalesNumber { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator")]
    public string? ScottishCharityRegulator { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator Number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-SC", ErrorMessage = "Please enter the Scottish Charity Regulator number.")]
    public string? ScottishCharityRegulatorNumber { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland")]
    public string? CharityCommissionNorthernIreland { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland Number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-NIC", ErrorMessage = "Please enter the Charity Commission for Northren Ireland number.")]
    public string? CharityCommissionNorthernIrelandNumber { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register")]
    public string? MutualsPublicRegister { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register Number")]
    [RequiredIf(nameof(OrganisationScheme), "GB-MPR", ErrorMessage = "Please enter the Mutuals Public Register number .")]
    public string? MutualsPublicRegisterNumber { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry")]
    public string? GuernseyRegistry { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry Number")]
    [RequiredIf(nameof(OrganisationScheme), "GG-RCE", ErrorMessage = "Please enter the Guernsey Registry number.")]
    public string? GuernseyRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry")]
    public string? JerseyFinancialServicesCommissionRegistry { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry Number")]
    [RequiredIf(nameof(OrganisationScheme), "JE-FSC", ErrorMessage = "Please enter Jersey Financial Services Commission Registry number")]
    public string? JerseyFinancialServicesCommissionRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry")]
    public string? IsleofManCompaniesRegistry { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry Number")]
    [RequiredIf(nameof(OrganisationScheme), "IM-CR", ErrorMessage = "Please enter the Isle of Man Companies Registry number.")]
    public string? IsleofManCompaniesRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("NHS Organisation Data Service (ODS)")]
    public string? NationalHealthServiceOrganisationsRegistry { get; set; }

    [BindProperty]
    [DisplayName("NHS Organisation Data Service (ODS)")]
    [RequiredIf(nameof(OrganisationScheme), "GB-NHS", ErrorMessage = "Please enter the NHS Organisation Data Service number.")]
    public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Providers (GB-UKPRN)")]
    public string? UKLearningProviderReference { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Providers (GB-UKPRN)")]
    [RequiredIf(nameof(OrganisationScheme), "GB-UKPRN", ErrorMessage = "Please enter the UK Register of Learning Providers number.")]
    public string? UKLearningProviderReferenceNumber { get; set; }

    [BindProperty]
    [DisplayName("VAT number")]
    public string? VAT { get; set; }

    [BindProperty]
    [DisplayName("VAT number")]
    [RequiredIf(nameof(OrganisationScheme), "VAT", ErrorMessage = "Please enter the VAT number.")]
    public string? VATNumber { get; set; }

    [BindProperty]
    [DisplayName("The organisation does not have a registry number")]
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
            case "VAT":
                VATNumber = RegistrationDetails.OrganisationIdentificationNumber;
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
            "VAT" => VATNumber,
            "Other" => null,
            _ => null,
        };

        try
        {
            var orgExists = await LookupOrganisationAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

            if (RedirectToSummary == true)
            {
                return RedirectToPage("OrganisationDetailsSummary");
            }
            else
            {
                return RedirectToPage("OrganisationName");
            }
        }

        return RedirectToPage("OrganisationAlreadyRegistered");
    }

    private async Task<Organisation.WebApiClient.Organisation> LookupOrganisationAsync()
    {
        return await organisationClient.LookupOrganisationAsync(string.Empty,
                    $"{OrganisationScheme}:{RegistrationDetails.OrganisationIdentificationNumber}");
    }
}