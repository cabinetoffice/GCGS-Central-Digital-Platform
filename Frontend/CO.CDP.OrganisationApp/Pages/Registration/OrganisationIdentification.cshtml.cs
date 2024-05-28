using CO.CDP.Common;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationIdentificationModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required(ErrorMessage = "Please select your organisation type")]
    public string? OrganisationScheme { get; set; }

    [BindProperty]
    [DisplayName("Companies House Number")]
    [RequiredIf("OrganisationType", "CHN")]
    public string? CompaniesHouseNumber { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for England & Wales")]
    public string? CharityCommissionEnglandWales { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for England & Wales Number")]
    [RequiredIf("OrganisationType", "CCEW")]
    public string? CharityCommissionEnglandWalesNumber { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator")]
    public string? ScottishCharityRegulator { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator Number")]
    [RequiredIf("OrganisationType", "SCR")]
    public string? ScottishCharityRegulatorNumber { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland")]
    public string? CharityCommissionNorthernIreland { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland Number")]
    [RequiredIf("OrganisationType", "CCNI")]
    public string? CharityCommissionNorthernIrelandNumber { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register")]
    public string? MutualsPublicRegister { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register Number")]
    [RequiredIf("OrganisationType", "MPR")]
    public string? MutualsPublicRegisterNumber { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry")]
    public string? GuernseyRegistry { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry Number")]
    [RequiredIf("OrganisationType", "GRN")]
    public string? GuernseyRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry")]
    public string? JerseyFinancialServicesCommissionRegistry { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry Number")]
    [RequiredIf("OrganisationType", "JFSC")]
    public string? JerseyFinancialServicesCommissionRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry")]
    public string? IsleofManCompaniesRegistry { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry Number")]
    [RequiredIf("OrganisationType", "IMCR")]
    public string? IsleofManCompaniesRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("NHS Organisation Data Service (ODS)")]
    public string? NationalHealthServiceOrganisationsRegistry { get; set; }

    [BindProperty]
    [DisplayName("NHS Organisation Data Service (ODS)")]
    [RequiredIf("OrganisationType", "NHOR")]
    public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Providers (UKPRN)")]
    public string? UKLearningProviderReference { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Providers (UKPRN)")]
    [RequiredIf("OrganisationType", "UKPRN")]
    public string? UKLearningProviderReferenceNumber { get; set; }

    [BindProperty]
    [DisplayName("VAT number")]
    public string? VAT { get; set; }

    [BindProperty]
    [DisplayName("VAT number")]
    [RequiredIf("OrganisationType", "VAT")]
    public string? VATNumber { get; set; }

    [BindProperty]
    [DisplayName("The organisation does not have a registry number")]
    public string? Other { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();

        OrganisationScheme = registrationDetails.OrganisationScheme;

        switch (OrganisationScheme)
        {
            case "CCEW":
                CharityCommissionEnglandWalesNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "SCR":
                ScottishCharityRegulatorNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "CCNI":
                CharityCommissionNorthernIrelandNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "MPR":
                MutualsPublicRegisterNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "GRN":
                GuernseyRegistryNumber = registrationDetails.OrganisationIdentificationNumber;
                break;

            case "JFSC":
                JerseyFinancialServicesCommissionRegistryNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "IMCR":
                IsleofManCompaniesRegistryNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "NHOR":
                NationalHealthServiceOrganisationsRegistryNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "UKPRN":
                UKLearningProviderReferenceNumber = registrationDetails.OrganisationIdentificationNumber;
                break;
            case "VAT":
                VATNumber = registrationDetails.OrganisationIdentificationNumber;
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

        var registrationDetails = VerifySession();
        registrationDetails.OrganisationScheme = OrganisationScheme;

        registrationDetails.OrganisationIdentificationNumber = OrganisationScheme switch
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
        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationName");
        }
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }

}
