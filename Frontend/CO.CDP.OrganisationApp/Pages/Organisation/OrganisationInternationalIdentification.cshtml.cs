using CO.CDP.EntityVerificationClient;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ApiException = CO.CDP.EntityVerificationClient.ApiException;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

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
        catch
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
            List<OrganisationIdentifier> identifiers = [new OrganisationIdentifier(
                    id: OrganisationIdentificationNumber?.Trim(),
                    legalName: organisation.Name,
                    scheme: OrganisationScheme)];

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

        return !((schemesInRegistries?.Count ?? 0) + 1 == validIdentifiers.Count);
    }
}
