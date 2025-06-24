using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
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
                var searchResults = await organisationClient.SearchOrganisationAsync(
                    name: Query,
                    role: "buyer",
                    limit: 20,
                    threshold: 0.3);

                if (searchResults != null && searchResults.Any())
                {
                    Results = searchResults
                        .Select(MapSearchResultToChildOrganisation)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }

    private ChildOrganisation MapSearchResultToChildOrganisation(OrganisationSearchResult searchResult)
    {
        return new ChildOrganisation(
            name: searchResult.Name,
            organisationId: searchResult.Id,
            identifier: searchResult.Identifier
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