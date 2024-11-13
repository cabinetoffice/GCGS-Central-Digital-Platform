using CO.CDP.EntityVerificationClient;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class OrganisationIdentificationModel(OrganisationWebApiClient.IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [DisplayName("Organisation Type")]
    [Required]
    public List<string> OrganisationScheme { get; set; } = [];

    [BindProperty]
    public List<string> ExistingOrganisationScheme { get; set; } = [];

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_CH_Label))]
    public string? CompanyHouse { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_CH_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GB-COH", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_CH_ErrorMessage))]
    public string? CompanyHouseNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_CHC_Label))]
    public string? CharityCommissionEnglandWales { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_CHC_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GB-CHC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_CHC_Number_ErrorMessage))]
    public string? CharityCommissionEnglandWalesNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_SC_Label))]
    public string? ScottishCharityRegulator { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_SC_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GB-SC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_SC_Number_ErrorMessage))]
    public string? ScottishCharityRegulatorNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NIC_Label))]
    public string? CharityCommissionNorthernIreland { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NIC_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GB-NIC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NIC_Number_ErrorMessage))]
    public string? CharityCommissionNorthernIrelandNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_MPR_Label))]
    public string? MutualsPublicRegister { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_MPR_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GB-MPR", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_MPR_Number_ErrorMessage))]
    public string? MutualsPublicRegisterNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GG_RCE_Label))]
    public string? GuernseyRegistry { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GG_RCE_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GG-RCE", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_GG_RCE_Number_ErrorMessage))]
    public string? GuernseyRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_JE_FSC_Label))]
    public string? JerseyFinancialServicesCommissionRegistry { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_JE_FSC_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "JE-FSC", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_JE_FSC_Number_ErrorMessage))]
    public string? JerseyFinancialServicesCommissionRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_IM_CR_Label))]
    public string? IsleofManCompaniesRegistry { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_IM_CR_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "IM-CR", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_IM_CR_Number_ErrorMessage))]
    public string? IsleofManCompaniesRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NHS_Label))]
    public string? NationalHealthServiceOrganisationsRegistry { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NHS_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GB-NHS", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_NHS_Number_ErrorMessage))]
    public string? NationalHealthServiceOrganisationsRegistryNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_UKPRN_Label))]
    public string? UKLearningProviderReference { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_UKPRN_Number_Label))]
    [RequiredIfContains(nameof(OrganisationScheme), "GB-UKPRN", ErrorMessage = nameof(StaticTextResource.Organisation_OrganisationIdentification_GB_UKPRN_Number_ErrorMessage))]
    public string? UKLearningProviderReferenceNumber { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_OrganisationIdentification_Other_Label))]
    public string? Other { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var (validate, existingIdentifier) = await ValidateAndGetExistingIdentifiers();
            if (!validate) return Redirect("/page-not-found");

            ExistingOrganisationScheme = existingIdentifier.Select(x => x.Scheme).ToList();

            foreach (var identifier in existingIdentifier)
            {
                SetIdentifierValue(identifier);
            }

            return Page();
        }
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    private async Task<(bool valid, List<OrganisationWebApiClient.Identifier>)> ValidateAndGetExistingIdentifiers()
    {
        var organisation = await organisationClient.GetOrganisationAsync(Id);
        if (organisation == null) return (false, new());

        var identfiers = organisation.AdditionalIdentifiers;
        identfiers.Add(organisation.Identifier);

        return (true, identfiers.ToList());
    }

    public async Task<IActionResult> OnPost()
    {
        // Ensure OrganisationScheme is valid
        if (OrganisationScheme == null || !OrganisationScheme.Any())
        {
            ModelState.AddModelError(nameof(OrganisationScheme), StaticTextResource.Organisation_OrganisationIdentification_ValidationErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            var (validate, existingIdentifier) = await ValidateAndGetExistingIdentifiers();
            if (!validate) return Redirect("/page-not-found");

            ExistingOrganisationScheme = existingIdentifier.Select(x => x.Scheme).ToList();

            return Page();
        }

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);
            if (organisation == null) return Redirect("/page-not-found");


            // Create identifiers for OrganisationScheme
            var identifiers = OrganisationScheme!.Select(scheme => new OrganisationWebApiClient.OrganisationIdentifier(
                    id: GetOrganisationIdentificationNumber(scheme),
                    legalName: organisation.Name,
                    scheme: scheme))
                .ToList();

            await organisationClient.UpdateOrganisationAdditionalIdentifiers(Id, identifiers);

            return RedirectToPage("OrganisationOverview", new { Id });
        }
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    private string? GetOrganisationIdentificationNumber(string scheme)
    {
        return scheme switch
        {
            "GB-COH" => CompanyHouseNumber,
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

    private void SetIdentifierValue(OrganisationWebApiClient.Identifier identifier)
    {
        switch (identifier.Scheme)
        {
            case "GB-COH":
                CompanyHouseNumber = identifier.LegalName;
                break;
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
}