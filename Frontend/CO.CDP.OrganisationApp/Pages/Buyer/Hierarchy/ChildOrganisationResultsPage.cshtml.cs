using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
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

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            return Page();
        }

        var (results, errorMessage, redirectToErrorPage) = await ExecuteSearch();

        if (redirectToErrorPage)
        {
            return RedirectToPage("/Error");
        }

        Results = results;
        ErrorMessage = errorMessage;

        return Page();
    }

    private ChildOrganisation MapOrganisationSearchResultToChildOrganisation(OrganisationSearchResult searchResult)
    {
        return new ChildOrganisation(
            name: searchResult.Name,
            organisationId: searchResult.Id,
            identifier: searchResult.Identifier
        );
    }

    private ChildOrganisation MapOrganisationLookupResultToChildOrganisation(
        CDP.Organisation.WebApiClient.Organisation organisation)
    {
        return new ChildOrganisation(
            name: organisation.Name,
            organisationId: organisation.Id,
            identifier: organisation.Identifier
        );
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Query))
        {
            return Page();
        }

        var (results, errorMessage, redirectToErrorPage) = await ExecuteSearch();

        if (redirectToErrorPage)
        {
            return RedirectToPage("/Error");
        }

        Results = results;
        ErrorMessage = errorMessage;

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

    private async Task<(List<ChildOrganisation> Results, string? ErrorMessage, bool RedirectToErrorPage)>
        ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            return (new List<ChildOrganisation>(), null, false);
        }

        try
        {
            var (isLikelyPpon, formattedPpon) = OrganisationIdentifierExtensions.IsLikelyPpon(Query);

            var (results, errorMessage) = await (isLikelyPpon
                ? ExecutePponSearch(formattedPpon)
                : ExecuteNameSearch());

            if (results.Count == 0)
            {
                return (results, errorMessage, false);
            }

            return results.Count > 0
                ? (results, null, false)
                : (new List<ChildOrganisation>(), StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults,
                    false);
        }
        catch (Exception ex) when (
            (ex is ApiException apiEx && apiEx.StatusCode != 404) ||
            (ex is HttpRequestException httpEx && httpEx.StatusCode != System.Net.HttpStatusCode.NotFound) ||
            (!(ex is ApiException) && !(ex is HttpRequestException)))
        {
            LogApiError(ex);
            return (new List<ChildOrganisation>(), null, true);
        }
    }

    private async Task<(List<ChildOrganisation> Results, string? ErrorMessage)> ExecutePponSearch(
        string? pponIdentifier)
    {
        if (string.IsNullOrWhiteSpace(pponIdentifier))
        {
            return (new List<ChildOrganisation>(),
                StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
        }

        var organisation = await OrganisationClientExtensions.LookupOrganisationAsync(_organisationClient,
            name: null,
            identifier: pponIdentifier);

        if (organisation == null ||
            organisation.Identifier.Scheme != "GB-PPON")
        {
            return (new List<ChildOrganisation>(),
                StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
        }

        return (new List<ChildOrganisation> { MapOrganisationLookupResultToChildOrganisation(organisation) }, null);
    }

    private async Task<(List<ChildOrganisation> Results, string? ErrorMessage)> ExecuteNameSearch()
    {
        var searchResults = await OrganisationClientExtensions.SearchOrganisationAsync(_organisationClient,
            name: Query,
            role: null,
            limit: 20,
            threshold: 0.3,
            true);

        if (searchResults.Count == 0)
        {
            return (new List<ChildOrganisation>(),
                StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
        }

        var results = searchResults
            .Where(r => r.Identifier.Scheme == "GB-PPON")
            .Select(MapOrganisationSearchResultToChildOrganisation)
            .ToList();

        return (results, null);
    }

    private void LogApiError(Exception ex)
    {
        var errorMessage = "Error occurred while searching for organisations";
        var errorCode = OrganisationIdentifierExtensions.IsLikelyPpon(Query).IsLikelyPpon ? "LOOKUP_ERROR" : "SEARCH_ERROR";
        var cdpException = new CdpExceptionLogging(errorMessage, errorCode, ex);
        _logger.LogError(cdpException, errorMessage);
    }
}