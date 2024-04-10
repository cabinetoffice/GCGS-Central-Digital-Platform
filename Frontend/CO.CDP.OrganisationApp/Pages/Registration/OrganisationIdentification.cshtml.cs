using CO.CDP.OrganisationApp.CustomeValidationAttributes;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OrganisationIdentificationModel(ISession session) : PageModel
{
    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required(ErrorMessage = "Please select your organisation type")]
    public string? OrganisationType { get; set; }

    //a. Companies House
    [BindProperty]
    [DisplayName("Companies House")]
    public string? CompaniesHouse { get; set; }

    [BindProperty]
    [DisplayName("Companies House Number")]
    [RequiredIf("OrganisationType", "CHN")]
    public string? CompaniesHouseNumber { get; set; }

    //b. Charity Commission for England and Wales
    [BindProperty]
    [DisplayName("Charity Commission for England & Wales")]
    public string? CharityCommissionEnglandWales { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for England & Wales Number")]
    [RequiredIf("OrganisationType", "CCEW")]
    public string? CharityCommissionEnglandWalesNumber { get; set; }

    //c. Scottish Charity Regulator
    [BindProperty]
    [DisplayName("Scottish Charity Regulator")]
    public string? ScottishCharityRegulator { get; set; }

    [BindProperty]
    [DisplayName("Scottish Charity Regulator Number")]
    [RequiredIf("OrganisationType", "SCR")]
    public string? ScottishCharityRegulatorNumber { get; set; }

    //d.Charity Commission for Northern Ireland
    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland")]
    public string? CharityCommissionNorthernIreland { get; set; }

    [BindProperty]
    [DisplayName("Charity Commission for Northren Ireland Number")]
    [RequiredIf("OrganisationType", "CCNI")]
    public string? CharityCommissionNorthernIrelandNumber { get; set; }

    //e. Mutuals Public Register
    [BindProperty]
    [DisplayName("Mutuals Public Register")]
    public string? MutualsPublicRegister { get; set; }

    [BindProperty]
    [DisplayName("Mutuals Public Register Number")]
    [RequiredIf("OrganisationType", "MPR")]
    public string? MutualsPublicRegisterNumber { get; set; }

    //f.Guernsey Registry
    [BindProperty]
    [DisplayName("Guernsey Registry")]
    public string? GuernseyRegistry { get; set; }

    [BindProperty]
    [DisplayName("Guernsey Registry Number")]
    [RequiredIf("OrganisationType", "GRN")]
    public string? GuernseyRegistryNumber { get; set; }

    //g. Jersey Financial Services Commission Registry
    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry")]
    public string? JerseyFinancialServicesCommissionRegistry { get; set; }

    [BindProperty]
    [DisplayName("Jersey Financial Services Commission Registry Number")]
    [RequiredIf("OrganisationType", "JFSC")]
    public string? JerseyFinancialServicesCommissionRegistryNumber { get; set; }

    //h. Isle of Man Companies Registry
    [BindProperty]
    [DisplayName("Isle of Man Companies Registry")]
    public string? IsleofManCompaniesRegistry { get; set; }

    [BindProperty]
    [DisplayName("Isle of Man Companies Registry Number")]
    [RequiredIf("OrganisationType", "IMCR")]
    public string? IsleofManCompaniesRegistryNumber { get; set; }

    //i. National Health Service Organisation Data Service
    [BindProperty]
    [DisplayName("National health Service Organisations Registry")]
    public string? NationalHealthServiceOrganisationsRegistry { get; set; }

    [BindProperty]
    [DisplayName("National health Service Organisations Registry Number")]
    [RequiredIf("OrganisationType", "NHOR")]
    public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

    //j. UK Register of Learning Providers (UKPRN number)
    [BindProperty]
    [DisplayName("UK Register of Learning Provider")]
    public string? UKLearningProviderReference { get; set; }

    [BindProperty]
    [DisplayName("UK Register of Learning Provider Number")]
    [RequiredIf("OrganisationType", "UKPRN")]
    public string? UKLearningProviderReferenceNumber { get; set; }

    //k. VAT number
    [BindProperty]
    [DisplayName("VAT number")]
    public string? VAT { get; set; }

    [BindProperty]
    [DisplayName("VAT number")]
    [RequiredIf("OrganisationType", "VAT")]
    public string? VATNumber { get; set; }

    [BindProperty]
    [DisplayName("Other / None")]
    public string? Other { get; set; }

    [BindProperty]
    [DisplayName("Other / None Number")]
    [RequiredIf("OrganisationType", "Other")]
    public string? OtherNumber { get; set; }

    public void OnGet()
    {
        var registrationDetails = VerifySession();

        OrganisationType = registrationDetails.OrganisationType;

        switch (OrganisationType)
        {
            case "CHN":
                CompaniesHouseNumber = registrationDetails.OrganisationIdentificationNumber;
                break;            
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
                JerseyFinancialServicesCommissionRegistryNumber = registrationDetails.OrganisationIdentificationNumber; ;
                break;
            case "IMCR":
                IsleofManCompaniesRegistryNumber = registrationDetails.OrganisationIdentificationNumber; ;
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
            case "Other":
                OtherNumber = registrationDetails.OrganisationIdentificationNumber;
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

        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);
        registrationDetails ??= new RegistrationDetails();
        registrationDetails.OrganisationType = OrganisationType;

        switch (OrganisationType)
        {
            case "CHN":
                registrationDetails.OrganisationIdentificationNumber = CompaniesHouseNumber;
                break;
            case "CCEW":
                registrationDetails.OrganisationIdentificationNumber = CharityCommissionEnglandWalesNumber;
                break;
            case "SCR":
                registrationDetails.OrganisationIdentificationNumber = ScottishCharityRegulatorNumber;
                break;
            case "CCNI":
                registrationDetails.OrganisationIdentificationNumber = CharityCommissionNorthernIrelandNumber;
                break;

            case "MPR":
                registrationDetails.OrganisationIdentificationNumber = MutualsPublicRegisterNumber;
                break;
            case "GRN":
                registrationDetails.OrganisationIdentificationNumber = GuernseyRegistryNumber;
                break;
            case "JFSC":
                registrationDetails.OrganisationIdentificationNumber = JerseyFinancialServicesCommissionRegistryNumber;
                break;
            case "IMCR":
                registrationDetails.OrganisationIdentificationNumber = IsleofManCompaniesRegistryNumber;
                break;


            case "NHOR":
                registrationDetails.OrganisationIdentificationNumber = NationalHealthServiceOrganisationsRegistryNumber;
                break;

            case "UKPRN":
                registrationDetails.OrganisationIdentificationNumber = UKLearningProviderReferenceNumber;
                break;
            case "VAT":
                registrationDetails.OrganisationIdentificationNumber = VATNumber;
                break;
            case "Other":
                registrationDetails.OrganisationIdentificationNumber = OtherNumber;
                break;
            default:
                registrationDetails.OrganisationIdentificationNumber = string.Empty;
                break;
        }

        session.Set(Session.RegistrationDetailsKey, registrationDetails);

        return RedirectToPage("./OrganisationRegisteredAddress");
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);
        if (registrationDetails == null)
        {
            //show error page (Once we finalise)
            throw new Exception("Shoudn't be here");
        }
        return registrationDetails;
    }

}
