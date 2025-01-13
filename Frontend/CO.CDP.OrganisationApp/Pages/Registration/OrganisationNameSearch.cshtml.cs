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
            var firstOrgResult = MatchingOrganisations.First();
            if (MatchingOrganisations.Count() == 1 && firstOrgResult.Name.ToLower() == OrganisationName?.ToLower())
            {
                flashMessageService.SetFlashMessage(
                    FlashMessageType.Important,
                    heading: StaticTextResource.OrganisationRegistration_SearchOrganisationName_ExactMatchAlreadyExists
                );

                return Redirect($"/registration/{Uri.EscapeDataString(firstOrgResult.Identifier.Scheme + ":" + firstOrgResult.Identifier.Id)}/join-organisation");
            }
        }

        return Page();
    }

    private async Task FindMatchingOrgs()
    {
        OrganisationName = RegistrationDetails.OrganisationName;
        MatchingOrganisations = await organisationClient.SearchOrganisationAsync(RegistrationDetails.OrganisationName, Constants.OrganisationType.Buyer.ToString(), 10);
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