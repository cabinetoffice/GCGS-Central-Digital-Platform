using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationResultsPage : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<OrganisationDto> Results { get; set; } = new();

    public Guid? SelectedOrganisationId { get; set; }

    public void OnGet()
    {
        if (!string.IsNullOrWhiteSpace(Query))
        {
            Results.AddRange(new[]
            {
                new OrganisationDto(
                    adminEmail: "admin@stark.com",
                    approvedOn: null,
                    contactPoints: new List<string>(),
                    id: Guid.NewGuid(),
                    identifiers: new List<string> { "DUNS: 123456789" },
                    name: "Stark Industries",
                    pendingRoles: new List<PartyRole>(),
                    reviewComment: null,
                    reviewedByFirstName: null,
                    reviewedByLastName: null,
                    roles: new List<PartyRole> { PartyRole.Buyer },
                    type: OrganisationType.Organisation
                ),
                new OrganisationDto(
                    adminEmail: "admin@wayne.com",
                    approvedOn: null,
                    contactPoints: new List<string>(),
                    id: Guid.NewGuid(),
                    identifiers: new List<string> { "DUNS: 987654321" },
                    name: "Wayne Enterprises",
                    pendingRoles: new List<PartyRole>(),
                    reviewComment: null,
                    reviewedByFirstName: null,
                    reviewedByLastName: null,
                    roles: new List<PartyRole> { PartyRole.Buyer },
                    type: OrganisationType.Organisation
                ),
                new OrganisationDto(
                    adminEmail: "admin@oscorp.com",
                    approvedOn: null,
                    contactPoints: new List<string>(),
                    id: Guid.NewGuid(),
                    identifiers: new List<string> { "DUNS: 555555555" },
                    name: "Oscorp",
                    pendingRoles: new List<PartyRole>(),
                    reviewComment: null,
                    reviewedByFirstName: null,
                    reviewedByLastName: null,
                    roles: new List<PartyRole> { PartyRole.Buyer },
                    type: OrganisationType.Organisation
                )
            });
        }
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