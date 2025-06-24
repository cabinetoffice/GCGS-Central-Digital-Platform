using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationResultsPage(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<ChildOrganisation> Results { get; set; } = new();

    [BindProperty]
    public Guid? SelectedOrganisationId { get; set; }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Query))
        {
            try
            {
                if (IsQueryAllNumeric(Query))
                {
                    var organisation = await organisationClient.LookupOrganisationAsync(
                        name: null,
                        identifier: Query);

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
                    var searchResults = await organisationClient.SearchOrganisationAsync(
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
            }
        }
    }

    private static bool IsQueryAllNumeric(string query) => query.All(char.IsDigit);

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

        return Page();
    }
}