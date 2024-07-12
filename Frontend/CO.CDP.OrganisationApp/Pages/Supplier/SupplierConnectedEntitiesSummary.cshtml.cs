using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntitiesSummaryModel(
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty] public ICollection<ConnectedEntityLookup> ConnectedEntities { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasConnectedEntity { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {
            ConnectedEntities = await organisationClient.GetConnectedEntities(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        HasConnectedEntity = selected;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        ConnectedEntities = await organisationClient.GetConnectedEntities(Id);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage(HasConnectedEntity == true ? "TODO" : "SupplierBasicInformation", new { Id });
    }
}