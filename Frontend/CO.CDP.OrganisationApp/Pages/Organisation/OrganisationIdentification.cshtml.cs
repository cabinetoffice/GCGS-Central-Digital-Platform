using CO.CDP.EntityVerificationClient;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ApiException = CO.CDP.EntityVerificationClient.ApiException;



namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class OrganisationIdentificationModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required(ErrorMessage = "Please select your organisation type")]
    public List<string> OrganisationScheme { get; set; } = [];

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
    [DisplayName("None apply")]
    public string? Other { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public required string Addorchange { get; set; }

    public async Task<IActionResult> OnGet()
    {

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);
            if (organisation == null) return Redirect("/page-not-found");

            OrganisationScheme = organisation.AdditionalIdentifiers.Select(x => x.Scheme).ToList();

            foreach (var identifier in organisation.AdditionalIdentifiers)
            {
                switch (identifier.Scheme)
                {
                    case "GB-CHC":
                        CharityCommissionEnglandWalesNumber = identifier.LegalName;
                        break;
                    case "GB-SC":
                        ScottishCharityRegulatorNumber = identifier.LegalName;
                        break;
                    case "GB-NIC":
                        CharityCommissionNorthernIrelandNumber = identifier.LegalName;
                        break;
                    case "GB-MPR":
                        MutualsPublicRegisterNumber = identifier.LegalName;
                        break;
                    case "GG-RCE":
                        GuernseyRegistryNumber = identifier.LegalName;
                        break;
                    case "JE-FSC":
                        JerseyFinancialServicesCommissionRegistryNumber = identifier.LegalName;
                        break;
                    case "IM-CR":
                        IsleofManCompaniesRegistryNumber = identifier.LegalName;
                        break;
                    case "GB-NHS":
                        NationalHealthServiceOrganisationsRegistryNumber = identifier.LegalName;
                        break;
                    case "GB-UKPRN":
                        UKLearningProviderReferenceNumber = identifier.LegalName;
                        break;
                    default:
                        break;
                }
            }

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Ensure OrganisationScheme is valid
        if (OrganisationScheme == null || OrganisationScheme.Count == 0)
        {
            return Redirect("/invalid-organisation-scheme");
        }
        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);
            if (organisation == null) return Redirect("/page-not-found");

            // Create a new list of identifiers based on the OrganisationScheme
            var identifiers = new List<OrganisationIdentifier>();

            foreach (var scheme in OrganisationScheme)
            {
                identifiers.Add(new OrganisationIdentifier(
                    id: null, 
                    legalName: GetLegalName(scheme),
                    scheme: scheme
                ));
            }

            await organisationClient.UpdateOrganisationAdditionalIdentifiers(Id, identifiers);

            return RedirectToPage("OrganisationOverview", new { Id });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    private string? GetLegalName(string scheme)
    {
        return scheme switch
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
    }
}