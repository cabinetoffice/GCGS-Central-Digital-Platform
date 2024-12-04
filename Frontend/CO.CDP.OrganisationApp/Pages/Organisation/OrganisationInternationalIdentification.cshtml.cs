using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.RazorPages;

using CO.CDP.EntityVerificationClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Constants;
using ApiException = CO.CDP.EntityVerificationClient.ApiException;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
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
    public bool HasIdentifierToShow { get; set; } = true;

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

    public async Task<IActionResult> OnGet()
    {
        if (string.IsNullOrEmpty(Country))
        { return RedirectToPage("OrganisationRegistrationUnavailable"); }

        try
        {
            var validate = await ValidateAndSetExistingIdentifiers();
            if (!validate) return Redirect("/page-not-found");
        }
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }



    public async Task<IActionResult> OnPost()
    {
        var OrganisationIdentificationNumber = OrganisationScheme != null ? RegistrationNumbers!.GetValueOrDefault(OrganisationScheme) : null;

        if (!ModelState.IsValid)
        {
            var validate = await ValidateAndSetExistingIdentifiers();
            if (!validate) return Redirect("/page-not-found");

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

    //private async Task<bool> ValidateAndSetExistingIdentifiers()
    //{
    //    var organisation = await organisationClient.GetOrganisationAsync(Id);
    //    if (organisation == null) return false;

    //    var identfiers = organisation.AdditionalIdentifiers;
    //    identfiers.Add(organisation.Identifier);

    //    ExistingInternationalIdentifiers = identfiers.Select(x => x.Scheme).ToList();

    //    InternationalIdentifiers = await PopulateIdentifierRegistries();

    //    // Check if ExistingInternationalIdentifiers exist in InternationalIdentifiers
    //    var schemesInRegistries = InternationalIdentifiers.Select(x => x.Scheme).ToHashSet();
    //    var existingInRegistries = ExistingInternationalIdentifiers.Where(scheme => schemesInRegistries.Contains(scheme) || scheme == Country + "-Other").ToList();

    //    if (schemesInRegistries.Any() && existingInRegistries.Count == 1)
    //    {
    //        HasIdentifierToShow = true;
    //    }

    //    return true;
    //}

    private async Task<bool> ValidateAndSetExistingIdentifiers()
    {
        // Fetch organization details
        var organisation = await organisationClient.GetOrganisationAsync(Id);
        if (organisation == null) return false;

        // Populate existing identifiers
        ExistingInternationalIdentifiers = PopulateExistingIdentifiers(organisation);

        // Load identifier registry data
        InternationalIdentifiers = await PopulateIdentifierRegistries();

        // Check and update identifier status
        HasIdentifierToShow = UpdateIdentifierStatus();

        return true;
    }

    private async Task<ICollection<IdentifierRegistries>> PopulateIdentifierRegistries()
    {
        try
        {
            return await pponClient.GetIdentifierRegistriesAsync(Country);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return new List<IdentifierRegistries>();
            // Show other
        }
    }

    private ICollection<string> PopulateExistingIdentifiers(OrganisationWebApiClient.Organisation organisation)
    {
        var identifiers = organisation.AdditionalIdentifiers ?? new List<OrganisationWebApiClient.Identifier>();
        identifiers.Add(organisation.Identifier);

        return identifiers.Select(x => x.Scheme).ToList();
    }

    private bool UpdateIdentifierStatus()
    {
        var schemesInRegistries = (InternationalIdentifiers?.Select(x => x.Scheme) ?? Enumerable.Empty<string>()).ToHashSet();

        var validIdentifiers = ExistingInternationalIdentifiers
            .Where(scheme => schemesInRegistries.Contains(scheme) || scheme == $"{Country}-Other")
            .ToList();

        return !(!schemesInRegistries.Any() && validIdentifiers.Count == 1);
    }
}
