using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.OrganisationApp.Models;
using Microsoft.Extensions.Logging;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationResultsPage(
    IOrganisationClient organisationClient,
    ILogger<ChildOrganisationResultsPage> logger)
    : PageModel
{
    private readonly IOrganisationClient _organisationClient = organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));
    private readonly ILogger<ChildOrganisationResultsPage> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<ChildOrganisation> Results { get; set; } = new();

    [BindProperty]
    public string? SelectedPponIdentifier { get; set; }

    [BindProperty]
    public Guid SelectedChildId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Query))
        {
            try
            {
                var (isPpon, pponIdentifier) = IsLikelyPpon(Query);
                if (isPpon)
                {
                    var organisation = await _organisationClient.LookupOrganisationAsync(
                        name: null,
                        identifier: pponIdentifier);

                    if (organisation != null)
                    {
                        Results = new List<ChildOrganisation>
                        {
                            MapOrganisationLookupResultToChildOrganisation(organisation)
                        };
                    }
                }
                else
                {
                    var searchResults = await _organisationClient.SearchOrganisationAsync(
                        name: Query,
                        role: "buyer",
                        limit: 20,
                        threshold: 0.3);

                    if (searchResults != null && searchResults.Any())
                    {
                        Results = searchResults
                            .Select(MapOrganisationSearchResultToChildOrganisation)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Error occurred while searching for organisations";
                var errorCode = IsLikelyPpon(Query).Item1 ? "LOOKUP_ERROR" : "SEARCH_ERROR";
                var cdpException = new CdpExceptionLogging(errorMessage, errorCode, ex);
                _logger.LogError(cdpException, errorMessage);
                return RedirectToPage("/Error");
            }
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

        var pponRegex = new System.Text.RegularExpressions.Regex("^[A-Z]{4}-\\d{4}-[A-Z]{4}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (pponRegex.IsMatch(query))
        {
            return (true, $"GB-PPON:{query}");
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

    private ChildOrganisation MapOrganisationLookupResultToChildOrganisation(CDP.Organisation.WebApiClient.Organisation organisation)
    {
        return new ChildOrganisation(
            name: organisation.Name,
            organisationId: organisation.Id,
            identifier: organisation.Identifier
        );
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("ChildOrganisationConfirmPage", new { Id, ChildId = SelectedChildId, Query });
    }
}