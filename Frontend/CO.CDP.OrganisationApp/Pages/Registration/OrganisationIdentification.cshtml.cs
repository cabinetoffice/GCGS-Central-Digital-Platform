using CO.CDP.Common;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
[ValidateRegistrationStep]
public class OrganisationIdentificationModel(ISession session) : RegistrationStepModel
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
    [RequiredIf(nameof(OrganisationScheme), "CCEW", ErrorMessage = "Please enter the Charity Commission for England & Wales number.")]
    public string? CharityCommissionEnglandWalesNumber { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator")]
    public string? ScottishCharityRegulator { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator Number")]
    [RequiredIf(nameof(OrganisationScheme), "SCR", ErrorMessage = "Please enter the Scottish Charity Regulator number.")]
    public string? ScottishCharityRegulatorNumber { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland")]
    public string? CharityCommissionNorthernIreland { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland Number")]
    [RequiredIf(nameof(OrganisationScheme), "CCNI", ErrorMessage = "Please enter the Charity Commission for Northren Ireland number.")]
    public string? CharityCommissionNorthernIrelandNumber { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register")]
    public string? MutualsPublicRegister { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register Number")]
    [RequiredIf(nameof(OrganisationScheme), "MPR", ErrorMessage = "Please enter the Mutuals Public Register number .")]
    public string? MutualsPublicRegisterNumber { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry")]
    public string? GuernseyRegistry { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry Number")]
    [RequiredIf(nameof(OrganisationScheme), "GRN", ErrorMessage = "Please enter the Guernsey Registry number.")]
    public string? GuernseyRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry")]
    public string? JerseyFinancialServicesCommissionRegistry { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry Number")]
    [RequiredIf(nameof(OrganisationScheme), "JFSC", ErrorMessage = "Please enter Jersey Financial Services Commission Registry number")]
    public string? JerseyFinancialServicesCommissionRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry")]
    public string? IsleofManCompaniesRegistry { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry Number")]
    [RequiredIf(nameof(OrganisationScheme), "IMCR", ErrorMessage = "Please enter the Isle of Man Companies Registry number.")]
    public string? IsleofManCompaniesRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("NHS Organisation Data Service (ODS)")]
    public string? NationalHealthServiceOrganisationsRegistry { get; set; }

    [BindProperty]
    [DisplayName("NHS Organisation Data Service (ODS)")]
    [RequiredIf(nameof(OrganisationScheme), "NHOR", ErrorMessage = "Please enter the NHS Organisation Data Service number.")]
    public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Providers (UKPRN)")]
    public string? UKLearningProviderReference { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Providers (UKPRN)")]
    [RequiredIf(nameof(OrganisationScheme), "UKPRN", ErrorMessage = "Please enter the UK Register of Learning Providers number.")]
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
            case "CCEW":
                CharityCommissionEnglandWalesNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "SCR":
                ScottishCharityRegulatorNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "CCNI":
                CharityCommissionNorthernIrelandNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "MPR":
                MutualsPublicRegisterNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "GRN":
                GuernseyRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;

            case "JFSC":
                JerseyFinancialServicesCommissionRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "IMCR":
                IsleofManCompaniesRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "NHOR":
                NationalHealthServiceOrganisationsRegistryNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "UKPRN":
                UKLearningProviderReferenceNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            case "VAT":
                VATNumber = RegistrationDetails.OrganisationIdentificationNumber;
                break;
            default:
                break;
        }

    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationScheme = OrganisationScheme;

        RegistrationDetails.OrganisationIdentificationNumber = OrganisationScheme switch
        {
            "CCEW" => CharityCommissionEnglandWalesNumber,
            "SCR" => ScottishCharityRegulatorNumber,
            "CCNI" => CharityCommissionNorthernIrelandNumber,
            "MPR" => MutualsPublicRegisterNumber,
            "GRN" => GuernseyRegistryNumber,
            "JFSC" => JerseyFinancialServicesCommissionRegistryNumber,
            "IMCR" => IsleofManCompaniesRegistryNumber,
            "NHOR" => NationalHealthServiceOrganisationsRegistryNumber,
            "UKPRN" => UKLearningProviderReferenceNumber,
            "VAT" => VATNumber,
            "Other" => string.Empty,
            _ => string.Empty,
        };
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
}