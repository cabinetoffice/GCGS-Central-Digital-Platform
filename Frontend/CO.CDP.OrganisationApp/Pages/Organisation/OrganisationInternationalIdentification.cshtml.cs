using CO.CDP.EntityVerificationClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;

using CO.CDP.OrganisationApp.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ApiException = CO.CDP.EntityVerificationClient.ApiException;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationInternationalIdentificationModel(
    IOrganisationClient organisationClient,
    IPponClient pponClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Country { get; set; }

    [BindProperty]
    public ICollection<IdentifierRegistries> InternationalIdentifiers { get; set; } = new List<IdentifierRegistries>();

    [BindProperty]
    public ICollection<string> ExistingInternationalIdentifiers { get; set; } = new List<string>();


    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Type_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Type_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? OrganisationScheme { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_RegistrationNumber_Heading))]
    [RequiredIfHasValue("OrganisationScheme", ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_RegistrationNumber_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public Dictionary<string, string?> RegistrationNumbers { get; set; } = new Dictionary<string, string?>();

    public string? Identifier { get; set; }

    public string? OrganisationName;

    public FlashMessage NotificationBannerCompanyAlreadyRegistered { get { return new FlashMessage(string.Format(StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyAlreadyRegistered_NotificationBanner, Identifier, OrganisationName)); } }

    public async Task<IActionResult> OnGet()
    {
        if (string.IsNullOrEmpty(Country))
        { return RedirectToPage("OrganisationRegistrationUnavailable"); }

        try
        {
            var (validate, existingIdentifier) = await ValidateAndGetExistingIdentifiers();
            if (!validate) return Redirect("/page-not-found");

            ExistingInternationalIdentifiers = existingIdentifier.Select(x => x.Scheme).ToList();            
        }
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);          
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        await IdentifierRegistries();
        return Page();
    }

    private async Task IdentifierRegistries()
    {
        try
        {
            InternationalIdentifiers = await pponClient.GetIdentifierRegistriesAsync(Country);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            // Show other
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var OrganisationIdentificationNumber = OrganisationScheme != null ? RegistrationNumbers!.GetValueOrDefault(OrganisationScheme) : null;
        Identifier = $"{OrganisationScheme}:{OrganisationIdentificationNumber}";        

        if (!ModelState.IsValid)
        {
            var (validate, existingIdentifier) = await ValidateAndGetExistingIdentifiers();
            if (!validate) return Redirect("/page-not-found");

            ExistingInternationalIdentifiers = existingIdentifier.Select(x => x.Scheme).ToList();

            await IdentifierRegistries();

            return Page();
        }

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);
            if (organisation == null) return Redirect("/page-not-found");

            // Create identifiers for OrganisationScheme
            var identifiers = OrganisationScheme!.Select(scheme => new OrganisationWebApiClient.OrganisationIdentifier(
                    id: OrganisationIdentificationNumber,
                    legalName: organisation.Name,
                    scheme: OrganisationScheme))
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
    private async Task<(bool valid, List<OrganisationWebApiClient.Identifier>)> ValidateAndGetExistingIdentifiers()
    {
        var organisation = await organisationClient.GetOrganisationAsync(Id);
        if (organisation == null) return (false, new());

        var identfiers = organisation.AdditionalIdentifiers;
        identfiers.Add(organisation.Identifier);

        return (true, identfiers.ToList());
    }

    private async Task<CO.CDP.Organisation.WebApiClient.Organisation> LookupOrganisationAsync()
    {
        return await organisationClient.LookupOrganisationAsync(string.Empty, Identifier);
    }

    private async Task<ICollection<EntityVerificationClient.Identifier>> LookupEntityVerificationAsync()
    {
        return await pponClient.GetIdentifiersAsync(Identifier);
    }
}
