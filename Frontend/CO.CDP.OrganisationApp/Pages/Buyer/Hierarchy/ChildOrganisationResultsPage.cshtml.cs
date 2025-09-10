using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.UI.Foundation.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = PolicyNames.PartyRole.BuyerWithSignedMou)]
[Authorize(Policy = OrgScopeRequirement.Editor)]
[FeatureGate(FeatureFlags.BuyerParentChildRelationship)]
public class ChildOrganisationResultsPage(
    IOrganisationClient organisationClient,
    ILogger<ChildOrganisationResultsPage> logger)
    : PageModel
{
    private readonly IOrganisationClient _organisationClient =
        organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));

    private readonly ILogger<ChildOrganisationResultsPage> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public string Query { get; set; } = string.Empty;

    public List<ChildOrganisation> Results { get; set; } = new();

    [BindProperty] public string? SelectedPponIdentifier { get; set; }

    [BindProperty] public Guid SelectedChildId { get; set; }

    public string? FeedbackMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Query = InputSanitiser.SanitiseSingleLineTextInput(Query) ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(Query))
        {
            return Page();
        }

        var (results, feedbackMessage, redirectToErrorPage) = await ExecuteSearch();

        if (redirectToErrorPage)
        {
            return RedirectToPage("/Error");
        }

        Results = results;
        FeedbackMessage = feedbackMessage;

        return Page();
    }


    private ChildOrganisation MapOrganisationSearchByPponResultToChildOrganisation(OrganisationSearchByPponResult searchResult)
    {
        var pponIdentifier = searchResult.Identifiers.FirstOrDefault(i => i.Scheme == "GB-PPON") ?? searchResult.Identifiers.First();
        return new ChildOrganisation(
            name: searchResult.Name,
            organisationId: searchResult.Id,
            identifier: pponIdentifier
        );
    }

    public async Task<IActionResult> OnPost()
    {
        Query = InputSanitiser.SanitiseSingleLineTextInput(Query) ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Query))
        {
            return Page();
        }

        var (results, feedbackMessage, redirectToErrorPage) = await ExecuteSearch();

        if (redirectToErrorPage)
        {
            return RedirectToPage("/Error");
        }

        Results = results;
        FeedbackMessage = feedbackMessage;

        if (Results.Count == 0)
        {
            return Page();
        }

        if (SelectedChildId == Guid.Empty)
        {
            ErrorMessage = StaticTextResource.Global_RadioField_SelectOptionError;
            return Page();
        }

        var selectedOrganisation = Results.FirstOrDefault(o => o.OrganisationId == SelectedChildId);

        return RedirectToPage("ChildOrganisationConfirmPage",
            new { Id, ChildId = SelectedChildId, Query, Ppon = selectedOrganisation?.GetIdentifierAsString() });
    }

    private async Task<(List<ChildOrganisation> Results, string? FeedbackMessage, bool RedirectToErrorPage)>
        ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            return (new List<ChildOrganisation>(), null, false);
        }

        try
        {
            var (results, feedbackMessage) = await ExecuteOrganisationSearch();

            if (results.Count == 0)
            {
                return (results, feedbackMessage, false);
            }

            var filteredResults = FilterResults(results);

            return filteredResults.Count > 0
                ? (filteredResults, null, false)
                : (new List<ChildOrganisation>(), StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults,
                    false);
        }
        catch (Exception ex)
        {
            LogApiError(ex);
            return (new List<ChildOrganisation>(), null, true);
        }
    }

    private List<ChildOrganisation> FilterResults(List<ChildOrganisation> results)
    {
        return results
            .Where(r => r.OrganisationId != Id)
            .ToList();
    }

    private async Task<(List<ChildOrganisation> Results, string? FeedbackMessage)> ExecuteOrganisationSearch()
    {
        var (searchResults, _) = await _organisationClient.SearchOrganisationByNameOrPpon(searchText: Query,
            pageSize: 20,
            skip: 0,
            orderBy: "rel",
            threshold: 0.2);

        if (searchResults.Count == 0)
        {
            return (new List<ChildOrganisation>(),
                StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
        }

        var results = searchResults
            .Where(r => r.Identifiers.Any(i => i.Scheme == "GB-PPON") &&
                        r.PartyRoles.Any(pr => pr.Role == PartyRole.Buyer))
            .Select(MapOrganisationSearchByPponResultToChildOrganisation)
            .ToList();

        return (results, null);
    }

    private void LogApiError(Exception ex)
    {
        var errorMessage = "Error occurred while searching for organisations";
        var cdpException = new CdpExceptionLogging(errorMessage, "SEARCH_ERROR", ex);
        _logger.LogError(cdpException, errorMessage);
    }
}