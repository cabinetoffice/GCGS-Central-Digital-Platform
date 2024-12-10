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

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationInternationalIdentificationModel(ISession session,
    IOrganisationClient organisationClient,
    IPponClient pponClient,
    IFlashMessageService flashMessageService) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationInternationalIdentifierPage;

    [BindProperty]
    public string? Country { get; set; }

    [BindProperty]
    public ICollection<IdentifierRegistries> InternationalIdentifiers { get; set; } = new List<IdentifierRegistries>();

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Type_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Type_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? OrganisationScheme { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_RegistrationNumber_Heading))]
    [RequiredIfHasValue("OrganisationScheme", ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_RegistrationNumber_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public Dictionary<string, string?> RegistrationNumbers { get; set; } = new Dictionary<string, string?>();

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public string? Identifier { get; set; }

    public string? OrganisationName;

    public async Task OnGet()
    {
        Country = RegistrationDetails.OrganisationIdentificationCountry;
        OrganisationScheme = RegistrationDetails.OrganisationScheme;
        if (!string.IsNullOrEmpty(OrganisationScheme))
        {
            RegistrationNumbers[OrganisationScheme] = RegistrationDetails.OrganisationIdentificationNumber;
        }

        await IdentifierRegistries();
    }

    private async Task IdentifierRegistries()
    {
        try
        {
            InternationalIdentifiers = await pponClient.GetIdentifierRegistriesAsync(Country);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            InternationalIdentifiers = new List<IdentifierRegistries>();
        }
        catch
        {
            RedirectToPage("OrganisationRegistrationUnavailable");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        Country = RegistrationDetails.OrganisationIdentificationCountry;
        RegistrationDetails.OrganisationScheme = OrganisationScheme;
        RegistrationDetails.OrganisationIdentificationNumber = OrganisationScheme != null ? RegistrationNumbers!.GetValueOrDefault(OrganisationScheme) : null;

        if (!ModelState.IsValid)
        {
            await IdentifierRegistries();
            return Page();
        }

        Identifier = $"{RegistrationDetails.OrganisationScheme}:{RegistrationDetails.OrganisationIdentificationNumber}";

        try
        {
            SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);
            var organisation = await LookupOrganisationAsync();
            OrganisationName = organisation?.Name;
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
                    return RedirectToPage("OrganisationName", new { InternationalIdentifier = true });
                }
            }
            catch
            {
                return RedirectToPage("OrganisationRegistrationUnavailable");
            }
        }

        if (OrganisationName != null)
        {
            flashMessageService.SetFlashMessage(
                FlashMessageType.Important,
                heading: StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyAlreadyRegistered_NotificationBanner,
                urlParameters: new() { ["organisationIdentifier"] = Identifier },
                htmlParameters: new() { ["organisationName"] = OrganisationName }
            );
        }

        return Page();
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
