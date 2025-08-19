using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
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
    public string? RequestToJoinOrganisationName { get; set; }

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

        MatchingOrganisations = await FindMatchingOrgs();

        if (MatchingOrganisations == null)
        {
            return RedirectToPage("OrganisationEmail");
        }

        var exactMatch = MatchingOrganisations.FirstOrDefault(o => o.Name.ToLower() == OrganisationName?.ToLower());
        if (exactMatch != null)
        {
            flashMessageService.SetFlashMessage(
                FlashMessageType.Important,
                heading: StaticTextResource.OrganisationRegistration_SearchOrganisationName_ExactMatchAlreadyExists
            );

                SessionContext.Set(Session.JoinOrganisationRequest,
                        new JoinOrganisationRequestState { OrganisationId = exactMatch.Id, OrganisationName = OrganisationName }
                        );

                return Redirect($"/registration/{exactMatch.Id.ToString()}/join-organisation");
        }

        return Page();
    }

    private async Task<ICollection<OrganisationSearchResult>?> FindMatchingOrgs()
    {
        OrganisationName = RegistrationDetails.OrganisationName;

        try
        {
            var existingOrg = await organisationClient.LookupOrganisationAsync(RegistrationDetails.OrganisationName, string.Empty);

            if (existingOrg != null)
            {
                var orgSearchResult = new OrganisationSearchResult(existingOrg.Id,
                    existingOrg.Identifier,
                    existingOrg.Name,
                    existingOrg.Details.PendingRoles,
                    existingOrg.Roles,
                    existingOrg.Type);

                return [orgSearchResult];
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            // Carry on - if we haven't found the org via a lookup we will try again via a fuzzy search
        }

        try
        {
            return await organisationClient.SearchOrganisationAsync(RegistrationDetails.OrganisationName, Constants.OrganisationType.Buyer.ToString(), 10, 0.3, false);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            MatchingOrganisations = await FindMatchingOrgs();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("OrganisationEmail");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if ((!String.IsNullOrEmpty(OrganisationIdentifier)) && (OrganisationIdentifier != "None"))
        {
            SessionContext.Set(Session.JoinOrganisationRequest,
                   new JoinOrganisationRequestState { OrganisationId = Guid.Parse(OrganisationIdentifier), OrganisationName = RequestToJoinOrganisationName }
                   );

            return RedirectToPage("JoinOrganisation", new { id = OrganisationIdentifier.ToString() });
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