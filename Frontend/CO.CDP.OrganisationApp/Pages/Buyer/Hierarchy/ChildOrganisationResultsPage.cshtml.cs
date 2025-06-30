using System.Net;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

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

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }

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

        try
        {
            var (results, errorMessage) = await ExecuteSearch();
            Results = results;
            ErrorMessage = errorMessage;
        }
        catch (Exception ex) when (!(ex is HttpRequestException notFoundEx &&
                                     notFoundEx.StatusCode == HttpStatusCode.NotFound))
        {
            LogApiError(ex);
            return RedirectToPage("/Error");
        }

        return Page();
    }

    private (bool, string?) IsLikelyPpon(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return (false, null);
        }

        if (query.StartsWith("GB-PPON:", StringComparison.OrdinalIgnoreCase))
        {
            return (true, query);
        }

        if (query.StartsWith("GB-PPON-", StringComparison.OrdinalIgnoreCase))
        {
            return (true, "GB-PPON:" + query.Substring("GB-PPON-".Length));
        }

        var pponRegex = new System.Text.RegularExpressions.Regex("^[A-Z]{4}-\\d{4}-[A-Z0-9]{4}$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (pponRegex.IsMatch(query))
        {
            return (true, "GB-PPON:" + query);
        }

        return (false, null);
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

        try
        {
            var (results, errorMessage) = await ExecuteSearch();
            Results = results;
            ErrorMessage = errorMessage;
        }
        catch (Exception ex) when (!(ex is HttpRequestException notFoundEx &&
                                     notFoundEx.StatusCode == HttpStatusCode.NotFound))
        {
            LogApiError(ex);
            return RedirectToPage("/Error");
        }

        if (Results.Count == 0)
        {
            return Page();
        }

        if (SelectedChildId == Guid.Empty)
        {
            ErrorMessage = StaticTextResource.Global_RadioField_SelectOptionError;
            return Page();
        }

        return RedirectToPage("ChildOrganisationConfirmPage", new { Id, ChildId = SelectedChildId, Query });
    }

    private async Task<(List<ChildOrganisation> Results, string? ErrorMessage)> ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(Query))
        {
            return (new List<ChildOrganisation>(), null);
        }

        var (isPpon, pponIdentifier) = IsLikelyPpon(Query);

        return isPpon
            ? await ExecutePponSearch(pponIdentifier)
            : await ExecuteNameSearch();
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

        return organisation != null
            ? (new List<ChildOrganisation> { MapOrganisationLookupResultToChildOrganisation(organisation) }, null)
            : (new List<ChildOrganisation>(), StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
    }

    private async Task<(List<ChildOrganisation> Results, string? ErrorMessage)> ExecuteNameSearch()
    {
        var searchResults = await OrganisationClientExtensions.SearchOrganisationAsync(_organisationClient,
            name: Query,
            role: "buyer",
            limit: 20,
            threshold: 0.3);

        return searchResults.Any()
            ? (searchResults.Select(MapOrganisationSearchResultToChildOrganisation).ToList(), null)
            : (new List<ChildOrganisation>(), StaticTextResource.BuyerParentChildRelationship_ResultsPage_NoResults);
    }

    private void LogApiError(Exception ex)
    {
        var errorMessage = "Error occurred while searching for organisations";
        var errorCode = IsLikelyPpon(Query).Item1 ? "LOOKUP_ERROR" : "SEARCH_ERROR";
        var cdpException = new CdpExceptionLogging(errorMessage, errorCode, ex);
        _logger.LogError(cdpException, errorMessage);
    }
}