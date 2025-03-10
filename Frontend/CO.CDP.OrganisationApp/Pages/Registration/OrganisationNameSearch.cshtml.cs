using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationNameSearchModel(ISession session, IOrganisationClient organisationClient, IFlashMessageService flashMessageService) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationNameSearchPage;

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_PleaseSelect), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? OrganisationIdentifier { get; set; }

    public string? OrganisationName { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }
    public ICollection<OrganisationSearchResult>? MatchingOrganisations { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (RegistrationDetails.OrganisationType != Constants.OrganisationType.Buyer
            || RegistrationDetails.OrganisationName?.Length < 3)
        {
            return RedirectToPage("OrganisationEmail");
        }

        try
        {
            await FindMatchingOrgs();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("OrganisationEmail");
        }

        if (MatchingOrganisations != null)
        {
            var exactMatch = MatchingOrganisations.FirstOrDefault(o => o.Name.ToLower() == OrganisationName?.ToLower());
            if (exactMatch != null)
            {
                flashMessageService.SetFlashMessage(
                    FlashMessageType.Important,
                    heading: StaticTextResource.OrganisationRegistration_SearchOrganisationName_ExactMatchAlreadyExists
                );

                return Redirect($"/registration/{Uri.EscapeDataString(exactMatch.Identifier.Scheme + ":" + exactMatch.Identifier.Id)}/join-organisation");
            }
        }

        return Page();
    }

    private async Task FindMatchingOrgs()
    {
        OrganisationName = RegistrationDetails.OrganisationName;

        var existingOrg = await organisationClient.LookupOrganisationAsync(RegistrationDetails.OrganisationName, string.Empty);

        if (existingOrg != null)
        {
            var orgSearchResult = new OrganisationSearchResult(existingOrg.Id,
                existingOrg.Identifier,
                existingOrg.Name,
                existingOrg.Roles,
                existingOrg.Type);

            MatchingOrganisations = [orgSearchResult];
        }
        else
        {
            MatchingOrganisations = await organisationClient.SearchOrganisationAsync(RegistrationDetails.OrganisationName, Constants.OrganisationType.Buyer.ToString(), 10, 0.3);
        }
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            await FindMatchingOrgs();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("OrganisationEmail");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!string.IsNullOrEmpty(OrganisationIdentifier) && OrganisationIdentifier != "None")
        {
            return Redirect($"/registration/{Uri.EscapeDataString(OrganisationIdentifier)}/join-organisation");
        }

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationEmail");
        }
    }
}